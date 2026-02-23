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
    public class PurchaseOrderController(DataContext context) : ControllerBase
    {
        private readonly DataContext _context = context;

        // CREAR ORDEN DE COMPRA (A partir de una Cotización Aprobada)
        [HttpPost]
        public async Task<ActionResult<ApiResponse<PurchaseOrderDetailDto>>> CreatePurchaseOrder([FromBody] CreatePurchaseOrderDto dto)
        {
            // Usamos transacción para asegurar que se cree la OC y se actualicen los estados de Quote/Purchase simultáneamente
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Validar que la Compra exista y no tenga ya una OC (Regla 1 a 1)
                var purchase = await _context.Purchase
                    .Include(p => p.PurchaseOrder)
                    .FirstOrDefaultAsync(p => p.Id == dto.PurchaseId);

                if (purchase == null)
                    return NotFound(new ApiResponse<PurchaseOrderDetailDto>(false, "La solicitud de compra no existe."));

                if (purchase.PurchaseOrder != null)
                    return Conflict(new ApiResponse<PurchaseOrderDetailDto>(false, "Esta compra ya tiene una Orden de Compra generada."));

                // 2. Obtener la Cotización con sus Items para calcular montos (Snapshot)
                var quote = await _context.Quotes
                    .Include(q => q.QuoteItems)
                    .FirstOrDefaultAsync(q => q.Id == dto.QuoteId);

                if (quote == null)
                    return NotFound(new ApiResponse<PurchaseOrderDetailDto>(false, "La cotización seleccionada no existe."));

                // Validación de integridad: La cotización debe pertenecer a la compra indicada
                if (quote.PurchaseId != dto.PurchaseId)
                    return BadRequest(new ApiResponse<PurchaseOrderDetailDto>(false, "La cotización no corresponde a la solicitud de compra indicada."));

                // 3. Generar Número de OC (Lógica simple: OC-{Año}-{Correlativo})
                // Nota: Para alta concurrencia real, considera usar una Sequence de base de datos o un servicio de folios.
                int count = await _context.PurchaseOrder.CountAsync(po => po.Date.Year == DateTime.UtcNow.Year);
                string orderNumber = $"OC-{DateTime.UtcNow.Year}-{(count + 1):D4}";

                // 4. Mapear DTO a Entidad (Datos básicos y logísticos)
                var newOrder = dto.ToModelFromCreate(orderNumber);

                // 5. CÁLCULO DE TOTALES (SNAPSHOT) - Vital para integridad financiera
                // No confiamos en lo que envíe el front, sumamos los items de la cotización real.
                decimal subTotal = 0;
                
                if (quote.QuoteItems != null)
                {
                    foreach (var item in quote.QuoteItems)
                    {
                        // Validamos que tenga precio unitario (no debería ser nulo si se va a aprobar)
                        decimal price = item.UnitPrice ?? 0;
                        decimal totalItem = price * item.Quantity;
                        subTotal += totalItem;
                    }
                }

                // Cálculos Impuestos (Chile 19%)
                decimal discount = 0; // Si implementas lógica de descuentos, va aquí
                decimal freight = 0; // Si implementas costo de flete en Quote, súmalo aquí
                
                // Asignar valores calculados al Snapshot de la OC
                newOrder.SubTotal = subTotal;
                newOrder.Discount = discount;
                newOrder.FreightCharge = freight;
                newOrder.TaxRate = 19m; // 19% Fijo por ahora
                
                // Base Imponible
                decimal taxableAmount = subTotal - discount + freight; 
                newOrder.TaxAmount = taxableAmount * 0.19m; 
                newOrder.TotalAmount = taxableAmount + newOrder.TaxAmount;

                // 6. Actualizar Estados Relacionados
                quote.Status = "Aprobada"; // La cotización gana
                purchase.Status = "Orden Generada"; // La compra avanza
                purchase.UpdatedAt = DateTime.UtcNow;

                // 7. Guardar en Base de Datos
                _context.PurchaseOrder.Add(newOrder);
                await _context.SaveChangesAsync();
                
                await transaction.CommitAsync();

                // 8. Retornar respuesta exitosa cargando las relaciones necesarias para el DTO de detalle
                // Hacemos una recarga explicita o re-consultamos para llenar el DTO completo con datos del proveedor
                var completeOrder = await _context.PurchaseOrder
                    .Include(po => po.Purchase)
                    .Include(po => po.Quote)
                        .ThenInclude(q => q.Supplier)
                    .Include(po => po.Quote)
                        .ThenInclude(q => q.QuoteItems)
                    .FirstAsync(po => po.Id == newOrder.Id);

                return CreatedAtAction(
                    nameof(GetPurchaseOrderById), 
                    new { id = newOrder.Id }, 
                    new ApiResponse<PurchaseOrderDetailDto>(true, "Orden de Compra generada exitosamente.", completeOrder.ToDetailDto())
                );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new ApiResponse<PurchaseOrderDetailDto>(false, "Error al generar la Orden de Compra: " + ex.Message));
            }
        }

        // OBTENER ORDEN DE COMPRA POR ID (Detalle Completo para Vista y PDF)
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<PurchaseOrderDetailDto>>> GetPurchaseOrderById(int id)
        {
            // Cargamos la Orden con TODAS sus relaciones necesarias para "aplanar" la data
            var purchaseOrder = await _context.PurchaseOrder
                .AsNoTracking() // Optimización: Solo lectura, no necesitamos tracking de cambios
                .Include(po => po.Purchase) // 1. Datos de la Obra/Solicitud
                .Include(po => po.Quote)    // 2. La Cotización Ganadora
                    .ThenInclude(q => q.Supplier) // 3. Datos del Proveedor (Anidados en Quote)
                .Include(po => po.Quote)
                    .ThenInclude(q => q.QuoteItems) // 4. Los Productos y Precios (Anidados en Quote)
                .FirstOrDefaultAsync(po => po.Id == id);

            if (purchaseOrder == null)
            {
                return NotFound(new ApiResponse<PurchaseOrderDetailDto>(
                    success: false, 
                    message: "Error al buscar.", 
                    errors: [$"No se encontró una Orden de Compra con el ID {id}."]
                ));
            }

            // Usamos el Mapper que creamos para convertir toda esa estructura jerárquica
            // en un objeto plano y fácil de usar para el Frontend y el PDF.
            return Ok(new ApiResponse<PurchaseOrderDetailDto>(
                success: true, 
                message: "Orden de Compra obtenida exitosamente.", 
                data: purchaseOrder.ToDetailDto()
            ));
        }


// OBTENER TODAS LAS ÓRDENES (Listado Paginado con Filtros)
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<PurchaseOrderSummaryDto>>>> GetAll([FromQuery] PurchaseOrderQueryParameters queryParams)
        {
            // 1. Iniciar consulta
            var query = _context.PurchaseOrder
                .AsNoTracking()
                .Include(po => po.Purchase)
                .Include(po => po.Quote)
                    .ThenInclude(q => q.Supplier)
                .AsQueryable();

            // 2. BÚSQUEDA GLOBAL (Search Term)
            if (!string.IsNullOrWhiteSpace(queryParams.Search))
            {
                var term = queryParams.Search.ToLower().Trim(); // Limpiamos espacios
                
                query = query.Where(po => 
                    po.OrderNumber.ToLower().Contains(term) || // CORREGIDO: Sin (char)
                    (po.Purchase != null && po.Purchase.ProjectName.ToLower().Contains(term)) || 
                    (po.Quote != null && po.Quote.Supplier != null && po.Quote.Supplier.BusinessName.ToLower().Contains(term))
                );
            }

            // 3. FILTRO POR ESTADO
            // Nota: Asumimos que po.Status es STRING. 
            // Si en tu base de datos Status es int (0, 1, 2), esta línea fallaría y habría que parsear.
            if (!string.IsNullOrWhiteSpace(queryParams.Status))
            {
                var status = queryParams.Status.ToLower().Trim();
                query = query.Where(po => po.Status.ToLower() == status);
            }

            // 4. FILTRO POR FECHA
            if (queryParams.StartDate.HasValue)
            {
                query = query.Where(po => po.Date >= queryParams.StartDate.Value);
            }

            if (queryParams.EndDate.HasValue)
            {
                // Ajustamos al final del día
                var endDate = queryParams.EndDate.Value.AddDays(1).AddTicks(-1);
                query = query.Where(po => po.Date <= endDate);
            }

            // 5. ORDENAMIENTO
            var sort = queryParams.SortBy?.ToLower() ?? "date_desc";

            query = sort switch
            {
                "date_asc" => query.OrderBy(po => po.Date),
                "date_desc" => query.OrderByDescending(po => po.Date),
                
                "amount_asc" => query.OrderBy(po => po.TotalAmount),
                "amount_desc" => query.OrderByDescending(po => po.TotalAmount),
                
                "supplier_asc" => query.OrderBy(po => po.Quote.Supplier.BusinessName),
                "supplier_desc" => query.OrderByDescending(po => po.Quote.Supplier.BusinessName),
                
                "project_asc" => query.OrderBy(po => po.Purchase.ProjectName),
                "project_desc" => query.OrderByDescending(po => po.Purchase.ProjectName),
                
                "status_asc" => query.OrderBy(po => po.Status),
                _ => query.OrderByDescending(po => po.Date)
            };

            // 6. PAGINACIÓN
            var totalItems = await query.CountAsync();

            var items = await query
                .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .ToListAsync();

            // 7. MAPEO
            var dtos = items.Select(po => po.ToSummaryDto()).ToList();

            // 8. RESPUESTA
            var pagedData = new PagedResponse<PurchaseOrderSummaryDto>(
                dtos, 
                totalItems, 
                queryParams.PageNumber, 
                queryParams.PageSize
            );

            return Ok(new ApiResponse<PagedResponse<PurchaseOrderSummaryDto>>(
                true, "Listado obtenido.", pagedData));
        }
    }
}