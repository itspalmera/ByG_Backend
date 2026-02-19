using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ByG_Backend.src.Data;
using ByG_Backend.src.DTOs;
using ByG_Backend.src.Helpers;
using ByG_Backend.src.Mappers;
using ByG_Backend.src.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using ByG_Backend.src.DTOs;

namespace ByG_Backend.src.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseController(DataContext context) : ControllerBase
    {
        private readonly DataContext _context = context;

        // OBTENER COMPRAS (CON BÚSQUEDA Y FILTROS)
        [HttpGet] // GET /api/purchase
        public async Task<ActionResult<ApiResponse<PagedResponse<PurchaseSummaryDto>>>> GetPurchases([FromQuery] PurchaseQueryParameters queryParams)
        {
            // 1. Iniciar consulta (Queryable)
            var query = _context.Purchase.AsNoTracking().AsQueryable();

            // 2. BÚSQUEDA GLOBAL (Search)
            if (!string.IsNullOrWhiteSpace(queryParams.Search))
            {
                var term = queryParams.Search.ToLower();
                
                // Busca coincidencias en: Folio, Nombre del Proyecto o Solicitante
                query = query.Where(p => 
                    p.PurchaseNumber.ToLower().Contains(term) ||
                    p.ProjectName.ToLower().Contains(term) ||
                    p.Requester.ToLower().Contains(term)
                );
            }

            // 3. FILTRO POR ESTADO (Status)
            if (!string.IsNullOrWhiteSpace(queryParams.Status))
            {
                var status = queryParams.Status.ToLower();
                query = query.Where(p => p.Status.ToLower() == status);
            }

            // 4. FILTRO POR RANGO DE FECHAS (RequestDate)
            if (queryParams.StartDate.HasValue)
            {
                query = query.Where(p => p.RequestDate >= queryParams.StartDate.Value);
            }

            if (queryParams.EndDate.HasValue)
            {
                // Ajustamos al final del día (23:59:59) para incluir todo el día seleccionado
                var endDate = queryParams.EndDate.Value.AddDays(1).AddTicks(-1);
                query = query.Where(p => p.RequestDate <= endDate);
            }

            // 5. ORDENAMIENTO DINÁMICO REFACTORIZADO
            var sort = queryParams.SortBy?.ToLower() ?? "date_desc"; // Si viene nulo, asume date_desc
            
            query = sort switch
            {
                "date_asc" => query.OrderBy(p => p.RequestDate),
                "date_desc" => query.OrderByDescending(p => p.RequestDate),
                "project_asc" => query.OrderBy(p => p.ProjectName),
                "project_desc" => query.OrderByDescending(p => p.ProjectName),
                "status_asc" => query.OrderBy(p => p.Status),
                _ => query.OrderByDescending(p => p.RequestDate) // Fallback seguro
            };

            // 6. Contar total antes de paginar (después de filtrar, antes de Skip/Take)
            var totalItems = await query.CountAsync();

            // 7. Aplicar Paginación y Proyección (SIN OrderBy aquí, ya se hizo arriba)
            var purchases = await query
                .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .Select(p => new PurchaseSummaryDto(
                    p.Id,
                    p.PurchaseNumber,
                    p.ProjectName,
                    p.Status,
                    p.RequestDate,
                    p.Requester,
                    p.PurchaseItems != null ? p.PurchaseItems.Count : 0 
                ))
                .ToListAsync();

            var pagedData = new PagedResponse<PurchaseSummaryDto>(purchases, totalItems, queryParams.PageNumber, queryParams.PageSize);

            return Ok(
                new ApiResponse<PagedResponse<PurchaseSummaryDto>>(
                    true, "Listado obtenido", pagedData
                )
            );
        }

        // OBTENER COMPRA POR ID (DETALLE)
        [HttpGet("{id}")] // GET /api/purchase/1
        public async Task<ActionResult<ApiResponse<PurchaseDetailDto>>> GetPurchaseById(int id)
        {
            // Para el detalle SÍ necesitamos incluir las relaciones hijas
            var purchase = await _context.Purchase
                .AsNoTracking()
                .Include(p => p.PurchaseItems) // Traemos los productos de la compra
                .Include(p => p.RequestQuote)  // Para saber si existe solicitud de cotización
                .Include(p => p.PurchaseOrder) // Para saber si existe orden de compra
                .FirstOrDefaultAsync(p => p.Id == id);

            if (purchase == null)
            {
                return NotFound(new ApiResponse<PurchaseDetailDto>(
                    success: false, message: "Error.", errors: [$"No se encontró la compra con ID {id}."]
                ));
            }

            return Ok(new ApiResponse<PurchaseDetailDto>(
                success: true,
                message: "Compra obtenida exitosamente.",
                data: purchase.ToDetailDto() // Usamos nuestro Mapper
            ));
        }

        // CREAR COMPRA (Desde Sistema Externo)
        [HttpPost] // POST /api/purchase
        public async Task<ActionResult<ApiResponse<PurchaseDetailDto>>> CreatePurchase([FromBody] PurchaseCreateDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Validaciones previas
                if (await _context.Purchase.AnyAsync(p => p.PurchaseNumber == dto.PurchaseNumber))
                    return Conflict(new ApiResponse<PurchaseDetailDto>(false, "El Folio ya existe."));

                // 2. Crear Compra
                var newPurchase = dto.ToModelFromCreate();
                newPurchase.Status = "Solicitud recibida"; // Estado inicial
                _context.Purchase.Add(newPurchase);
                await _context.SaveChangesAsync();

                // 3. Crear RequestQuote (Siempre 1 a 1 con Purchase)
                var requestQuote = new RequestQuote
                {
                    PurchaseId = newPurchase.Id,
                    Number = newPurchase.PurchaseNumber.Replace("REQ", "RFQ"),
                    Status = "Pendiente",
                    CreatedAt = DateTime.UtcNow
                };
                _context.RequestQuotes.Add(requestQuote);
                await _context.SaveChangesAsync();

                // 4. Si el usuario envió proveedores seleccionados, los vinculamos de inmediato
                if (dto.InitialSupplierIds != null && dto.InitialSupplierIds.Any())
                {
                    var relations = dto.InitialSupplierIds.Select(sId => new RequestQuoteSupplier
                    {
                        RequestQuoteId = requestQuote.Id,
                        SupplierId = sId,
                        SentAt = DateTime.UtcNow // O dejar nulo hasta que se envíe el correo realmente
                    });
                    _context.RequestQuoteSuppliers.AddRange(relations);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return CreatedAtAction(nameof(GetPurchaseById), new { id = newPurchase.Id }, 
                    new ApiResponse<PurchaseDetailDto>(true, "Compra y Solicitud iniciada.", newPurchase.ToDetailDto()));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new ApiResponse<PurchaseDetailDto>(false, "Error: " + ex.Message));
            }
        }
        // ACTUALIZAR DATOS DE LA COMPRA (Cabecera)
        [HttpPut("{id}")] // PUT /api/purchase/1
        public async Task<ActionResult<ApiResponse<PurchaseDetailDto>>> UpdatePurchase(int id, [FromBody] PurchaseUpdateDto dto)
        {
            var purchase = await _context.Purchase
                .Include(p => p.PurchaseItems) 
                .Include(p => p.RequestQuote)  // <-- FALTABA ESTO
                .Include(p => p.PurchaseOrder) // <-- FALTABA ESTO
                .FirstOrDefaultAsync(p => p.Id == id);

            if (purchase == null)
            {
                return NotFound(new ApiResponse<PurchaseDetailDto>(
                    success: false, message: "Error.", errors: [$"No se encontró la compra con ID {id}."]
                ));
            }

            purchase.UpdateModel(dto);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<PurchaseDetailDto>(
                success: true, message: "Compra actualizada exitosamente.", data: purchase.ToDetailDto()
            ));
        }

        // ACTUALIZAR ESTADO DE LA COMPRA (Flujo de trabajo)
        [HttpPatch("{id}/status")] // PATCH /api/purchase/1/status
        public async Task<ActionResult<ApiResponse<string>>> UpdateStatusPurchase(int id, [FromBody] string newStatus)
        {
            // Validar que el estado enviado no sea nulo o vacío
            if (string.IsNullOrWhiteSpace(newStatus))
            {
                return BadRequest(new ApiResponse<string>(
                    success: false, message: "Error.", errors: ["El nuevo estado no puede estar vacío."]
                ));
            }

            var purchase = await _context.Purchase.FindAsync(id);

            if (purchase == null)
            {
                return NotFound(new ApiResponse<string>(
                    success: false, message: "Error.", errors: [$"No se encontró la compra con ID {id}."]
                ));
            }

            // Actualizamos el estado y la fecha de modificación
            purchase.Status = newStatus;
            purchase.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<string>(
                success: true, message: $"El estado de la compra se actualizó a: {newStatus}"
            ));
        }

        // ELIMINAR COMPRA (Hard Delete, sólo si ocurre un error y no hay flujo avanzado)
        [HttpDelete("{id}")] // DELETE /api/purchase/1
        public async Task<ActionResult<ApiResponse<string>>> DeletePurchase(int id)
        {
            // Evitar eliminar compras que ya están en proceso de cotización
            bool hasWorkflowStarted = await _context.Purchase
                .AsNoTracking()
                .AnyAsync(p => p.Id == id && (p.Quotes.Count != 0 || p.RequestQuote != null));

            if (hasWorkflowStarted)
            {
                return Conflict(new ApiResponse<string>(
                    success: false, 
                    message: "No se puede eliminar.", 
                    errors: ["La compra ya tiene un proceso de cotización iniciado o finalizado. Utilice el cambio de estado para Cancelarla."]
                ));
            }

            // Eliminación optimizada (eliminará los PurchaseItems automáticamente si tienes la Cascade Delete en EF Core, lo cual es el default)
            int deletedRows = await _context.Purchase.Where(p => p.Id == id).ExecuteDeleteAsync();

            if (deletedRows == 0)
            {
                return NotFound(new ApiResponse<string>(
                    success: false, message: "Error.", errors: [$"No se encontró la compra con ID {id}."]
                ));
            }

            return Ok(new ApiResponse<string>(
                success: true, message: "Compra eliminada definitivamente."
            ));
        }
    

        // AGREGAR PROVEEDORES
        [HttpPost("{purchaseId}/add-suppliers")]
        public async Task<ActionResult<ApiResponse<string>>> AddSuppliersToQuote(int purchaseId, [FromBody] List<int> supplierIds)
        {
            // 1. Buscar la RequestQuote asociada a esa compra
            var requestQuote = await _context.RequestQuotes
                .FirstOrDefaultAsync(rq => rq.PurchaseId == purchaseId);

            if (requestQuote == null) return NotFound(new ApiResponse<string>(false, "No existe solicitud de cotización."));

            // 2. Filtrar proveedores que ya están agregados para evitar duplicados
            var existingIds = await _context.RequestQuoteSuppliers
                .Where(rqs => rqs.RequestQuoteId == requestQuote.Id)
                .Select(rqs => rqs.SupplierId)
                .ToListAsync();

            var newIds = supplierIds.Except(existingIds).ToList();

            if (!newIds.Any()) return BadRequest(new ApiResponse<string>(false, "Los proveedores ya están en la lista."));

            // 3. Agregar los nuevos
            var newRelations = newIds.Select(sId => new RequestQuoteSupplier
            {
                RequestQuoteId = requestQuote.Id,
                SupplierId = sId,
                SentAt = DateTime.UtcNow
            });

            _context.RequestQuoteSuppliers.AddRange(newRelations);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<string>(true, "Proveedores agregados exitosamente."));
        }

    }
}