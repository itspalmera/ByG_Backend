using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using ByG_Backend.src.Data;
using ByG_Backend.src.DTOs;
using ByG_Backend.src.Helpers;
using ByG_Backend.src.RequestHelpers;
using ByG_Backend.src.Mappers;
using ByG_Backend.src.Models;

namespace ByG_Backend.src.Controller
{
    /// <summary>
    /// Controlador para la gestión de las compras.
    /// Permite el listado paginado, creación transaccional y gestión de proveedores asociados.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseController(DataContext context) : ControllerBase
    {
        /// <summary>
        /// Obtiene un listado paginado de compras con soporte para filtros, búsqueda y ordenamiento.
        /// </summary>
        /// <param name="queryParams">Parámetros de consulta: Status, fechas, término de búsqueda, orden y paginación.</param>
        /// <returns>Objeto ApiResponse con una página de resúmenes de compra (PurchaseSummaryDto).</returns>
        [HttpGet] 
        public async Task<ActionResult<ApiResponse<PagedResponse<PurchaseSummaryDto>>>> GetPurchases([FromQuery] PurchaseQueryParameters queryParams)
        {
            try
            {
                var query = context.Purchase.AsNoTracking().AsQueryable();

                if (!string.IsNullOrWhiteSpace(queryParams.Status))
                {
                    query = query.Where(p => p.Status.ToLower() == queryParams.Status.ToLower());
                }

                if (queryParams.StartDate.HasValue)
                {
                    query = query.Where(p => p.RequestDate >= queryParams.StartDate.Value);
                }

                if (queryParams.EndDate.HasValue)
                {
                    var endDate = queryParams.EndDate.Value.AddDays(1).AddTicks(-1);
                    query = query.Where(p => p.RequestDate <= endDate);
                }

                query = query.ApplySearch(queryParams.Search, "PurchaseNumber", "ProjectName", "Requester");

                query = query.ApplySorting(queryParams.SortBy, "RequestDate:desc");

                var pagedResult = await query.ToPagedResponseAsync(queryParams.PageNumber, queryParams.PageSize);

                var dtos = pagedResult.Items.Select(p => new PurchaseSummaryDto(
                    p.Id,
                    p.PurchaseNumber,
                    p.ProjectName,
                    p.Status,
                    p.RequestDate,
                    p.Requester,
                    p.PurchaseItems != null ? p.PurchaseItems.Count : 0 
                )).ToList();

                var response = new PagedResponse<PurchaseSummaryDto>(dtos, pagedResult.TotalItems, pagedResult.PageNumber, pagedResult.PageSize);

                return Ok(new ApiResponse<PagedResponse<PurchaseSummaryDto>>(true, "Listado obtenido", response));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>(false, "Error: " + ex.Message));
            }
        }

        /// <summary>
        /// Obtiene el detalle completo de una compra específica por su ID.
        /// Incluye ítems, solicitudes de cotización y órdenes de compra relacionadas.
        /// </summary>
        /// <param name="id">Identificador único de la compra.</param>
        /// <returns>Detalle de la compra o 404 si no existe.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<PurchaseDetailDto>>> GetPurchaseById(int id)
        {
            var purchase = await context.Purchase
                .AsNoTracking()
                .Include(p => p.PurchaseItems)
                .Include(p => p.RequestQuote)
                    .ThenInclude(rq => rq.RequestQuoteSuppliers)
                        .ThenInclude(rqs => rqs.Supplier) // <--- ESTO ES CLAVE para traer el nombre
                .Include(p => p.PurchaseOrder)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (purchase == null)
            {
                return NotFound(new ApiResponse<PurchaseDetailDto>(false, "Compra no encontrada."));
            }

            return Ok(new ApiResponse<PurchaseDetailDto>(true, "Compra obtenida.", purchase.ToDetailDto()));
        }

        /// <summary>
        /// Inicia un nuevo proceso de compra de forma transaccional.
        /// </summary>
        /// <remarks>
        /// Este endpoint realiza múltiples pasos:
        /// 1. Crea el registro de la compra (Purchase).
        /// 2. Genera automáticamente una solicitud de cotización (RequestQuote) con prefijo "RFQ".
        /// 3. Si se envían proveedores iniciales, crea las relaciones correspondientes.
        /// </remarks>
        /// <param name="dto">Datos para la creación de la compra y proveedores opcionales.</param>
        /// <returns>La compra creada y su ubicación en la API.</returns>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<PurchaseDetailDto>>> CreatePurchase([FromBody] PurchaseCreateDto dto)
        {
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                if (await context.Purchase.AnyAsync(p => p.PurchaseNumber == dto.PurchaseNumber))
                    return Conflict(new ApiResponse<PurchaseDetailDto>(false, "El Folio ya existe."));

                var newPurchase = dto.ToModelFromCreate();
                newPurchase.Status = "Solicitud recibida";
                context.Purchase.Add(newPurchase);
                await context.SaveChangesAsync();

                var requestQuote = new RequestQuote
                {
                    PurchaseId = newPurchase.Id,
                    Number = newPurchase.PurchaseNumber.Replace("REQ", "RFQ"),
                    Status = "Pendiente",
                    CreatedAt = DateTime.UtcNow
                };
                context.RequestQuotes.Add(requestQuote);
                await context.SaveChangesAsync();

                if (dto.InitialSupplierIds != null && dto.InitialSupplierIds.Any())
                {
                    var relations = dto.InitialSupplierIds.Select(sId => new RequestQuoteSupplier
                    {
                        RequestQuoteId = requestQuote.Id,
                        SupplierId = sId
                        // Tampoco ponemos SentAt aquí.
                    });
                    context.RequestQuoteSuppliers.AddRange(relations);
                    await context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return CreatedAtAction(nameof(GetPurchaseById), new { id = newPurchase.Id }, 
                    new ApiResponse<PurchaseDetailDto>(true, "Compra iniciada.", newPurchase.ToDetailDto()));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new ApiResponse<string>(false, "Error: " + ex.Message));
            }
        }


        // ============================================================
        // CREAR COMPRA AUTOMÁTICA DESDE SISTEMA DE SOLICITUDES
        // ============================================================
        /// <summary>
        /// Genera automáticamente un requerimiento de compra a partir de una solicitud externa aprobada.
        /// </summary>
        /// <remarks>
        /// Este endpoint realiza un mapeo complejo:
        /// 1. Extrae datos de la tabla 'Solicitudes' (sistema externo).
        /// 2. Valida la existencia previa para evitar duplicados mediante el folio formateado (REQ-XXXX).
        /// 3. Transforma los detalles de bodega en ítems de compra con especificaciones técnicas.
        /// 4. Inicia automáticamente el flujo de licitación creando una RFQ (Request for Quotation).
        /// </remarks>
        /// <param name="solicitudId">Identificador único de la solicitud en el sistema de origen.</param>
        /// <returns>Mensaje de éxito con el folio generado o error detallado en caso de fallo transaccional.</returns>
        [HttpPost("from-solicitud/{solicitudId}")]
        public async Task<ActionResult<ApiResponse<PurchaseDetailDto>>> CreateFromSolicitud(int solicitudId)
        {
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var solicitud = await context.Solicitudes
                    .Include(s => s.UsuarioSolicitante)
                    .Include(s => s.Detalles)
                        .ThenInclude(d => d.Producto) 
                    .FirstOrDefaultAsync(s => s.Id == solicitudId);

                if (solicitud == null)
                    return NotFound(new ApiResponse<string>(false, "La Solicitud indicada no existe en el sistema externo."));

                string folioCompra = $"REQ-{solicitud.Folio:D4}";
                if (await context.Purchase.AsNoTracking().AnyAsync(p => p.PurchaseNumber == folioCompra))
                    return Conflict(new ApiResponse<string>(false, $"La solicitud {folioCompra} ya ha sido importada anteriormente."));

                var newPurchase = new Purchase
                {
                    PurchaseNumber = folioCompra,
                    ProjectName = string.IsNullOrWhiteSpace(solicitud.Proyecto) ? "Proyecto General" : solicitud.Proyecto,
                    Status = "Esperando proveedores", 
                    RequestDate = solicitud.FechaCreacion,
                    UpdatedAt = DateTime.UtcNow,


                    Requester = solicitud.UsuarioSolicitanteId != null
                    ? $"{solicitud.UsuarioSolicitante.FirstName} {solicitud.UsuarioSolicitante.LastName}"
                    : (solicitud.UsuarioSolicitanteId ?? "Sistema Automático"),
                    Observations = solicitud.Observaciones,
                    
                    PurchaseItems = solicitud.Detalles.Select(d => new PurchaseItem
                    {
                        Name = d.Producto != null ? d.Producto.NombreProducto : (d.TemporalNombre ?? "Producto sin especificar"),
                        Description = d.Observacion,
                        Unit = d.Producto != null ? (d.Producto.Formato ?? "UN") : (d.TemporalUnidad ?? "UN"),
                        Size = d.Producto != null ? d.Producto.TallaMedida : d.TemporalTalla,
                        Quantity = d.CantidadAprobada > 0 ? d.CantidadAprobada : d.CantidadSolicitada
                    }).ToList()
                };

                context.Purchase.Add(newPurchase);
                await context.SaveChangesAsync(); 

                var requestQuote = new RequestQuote
                {
                    PurchaseId = newPurchase.Id,
                    Number = newPurchase.PurchaseNumber.Replace("REQ", "RFQ"),
                    Status = "Pendiente",
                    CreatedAt = DateTime.UtcNow
                };
                context.RequestQuotes.Add(requestQuote);

                await context.SaveChangesAsync();
                
                await transaction.CommitAsync();

                return Ok(new ApiResponse<string>(true, $"Compra {folioCompra} generada exitosamente desde la Solicitud."));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new ApiResponse<string>(false, "Error crítico al generar compra: " + ex.Message));
            }
        }

        /// <summary>
        /// Actualiza la información general de una compra y sus ítems.
        /// </summary>
        /// <param name="id">ID de la compra a actualizar.</param>
        /// <param name="dto">Nuevos datos de la compra.</param>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<PurchaseDetailDto>>> UpdatePurchase(int id, [FromBody] PurchaseUpdateDto dto)
        {
            var purchase = await context.Purchase
                .Include(p => p.PurchaseItems) 
                .Include(p => p.RequestQuote)
                .Include(p => p.PurchaseOrder)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (purchase == null) return NotFound(new ApiResponse<string>(false, "No encontrada."));

            purchase.UpdateModel(dto);
            await context.SaveChangesAsync();

            return Ok(new ApiResponse<PurchaseDetailDto>(true, "Compra actualizada.", purchase.ToDetailDto()));
        }

        /// <summary>
        /// Actualiza únicamente el estado de una compra.
        /// </summary>
        /// <param name="id">ID de la compra.</param>
        /// <param name="newStatus">Cadena de texto con el nuevo estado.</param>
        [HttpPatch("{id}/status")]
        public async Task<ActionResult<ApiResponse<string>>> UpdateStatusPurchase(int id, [FromBody] string newStatus)
        {
            if (string.IsNullOrWhiteSpace(newStatus)) return BadRequest(new ApiResponse<string>(false, "Estado inválido."));

            var purchase = await context.Purchase.FindAsync(id);
            if (purchase == null) return NotFound(new ApiResponse<string>(false, "No encontrada."));

            purchase.Status = newStatus;
            purchase.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();

            return Ok(new ApiResponse<string>(true, $"Estado actualizado a: {newStatus}"));
        }

        /// <summary>
        /// Elimina una compra si esta no ha iniciado un flujo de trabajo (cotizaciones u órdenes).
        /// </summary>
        /// <param name="id">ID de la compra a eliminar.</param>
        /// <returns>Resultado de la operación o Conflicto si el proceso ya inició.</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<string>>> DeletePurchase(int id)
        {
            bool hasWorkflowStarted = await context.Purchase
                .AsNoTracking()
                .AnyAsync(p => p.Id == id && (p.Quotes.Count != 0 || p.RequestQuote != null));

            if (hasWorkflowStarted)
                return Conflict(new ApiResponse<string>(false, "No se puede eliminar una compra en proceso."));

            int deletedRows = await context.Purchase.Where(p => p.Id == id).ExecuteDeleteAsync();
            if (deletedRows == 0) return NotFound();

            return Ok(new ApiResponse<string>(true, "Compra eliminada."));
        }

        /// <summary>
        /// Agrega nuevos proveedores a la solicitud de cotización asociada a una compra.
        /// Evita la duplicidad de proveedores en la misma solicitud.
        /// </summary>
        /// <param name="purchaseId">ID de la compra.</param>
        /// <param name="supplierIds">Lista de IDs de proveedores a invitar.</param>
        [HttpPost("{purchaseId}/add-suppliers")]
        public async Task<ActionResult<ApiResponse<string>>> AddSuppliersToQuote(int purchaseId, [FromBody] List<int> supplierIds)
        {
            var requestQuote = await context.RequestQuotes.FirstOrDefaultAsync(rq => rq.PurchaseId == purchaseId);
            if (requestQuote == null) return NotFound(new ApiResponse<string>(false, "No existe solicitud."));

            var existingIds = await context.RequestQuoteSuppliers
                .Where(rqs => rqs.RequestQuoteId == requestQuote.Id)
                .Select(rqs => rqs.SupplierId)
                .ToListAsync();

            var newIds = supplierIds.Except(existingIds).ToList();
            if (!newIds.Any()) return BadRequest(new ApiResponse<string>(false, "Ya están en la lista."));

            var newRelations = newIds.Select(sId => new RequestQuoteSupplier
            {
                RequestQuoteId = requestQuote.Id,
                SupplierId = sId
                // Ya no ponemos SentAt aquí. Se quedará nulo o por defecto (0001) hasta que se envíe el correo.
            });

            context.RequestQuoteSuppliers.AddRange(newRelations);
            await context.SaveChangesAsync();

            return Ok(new ApiResponse<string>(true, "Proveedores agregados."));
        }
    
        /// <summary>
        /// Elimina un proveedor específico de una solicitud de cotización.
        /// </summary>
        [HttpDelete("{purchaseId}/remove-supplier/{supplierId}")]
        public async Task<ActionResult<ApiResponse<string>>> RemoveSupplierFromQuote(int purchaseId, int supplierId)
        {
            var requestQuote = await context.RequestQuotes.FirstOrDefaultAsync(rq => rq.PurchaseId == purchaseId);
            if (requestQuote == null) return NotFound(new ApiResponse<string>(false, "No existe solicitud para esta compra."));

            var relation = await context.RequestQuoteSuppliers
                .FirstOrDefaultAsync(rqs => rqs.RequestQuoteId == requestQuote.Id && rqs.SupplierId == supplierId);

            if (relation == null) return NotFound(new ApiResponse<string>(false, "El proveedor no está en la lista."));

            context.RequestQuoteSuppliers.Remove(relation);
            await context.SaveChangesAsync();

            return Ok(new ApiResponse<string>(true, "Proveedor removido correctamente."));
        }
    }
}