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
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseOrderController(DataContext context) : ControllerBase
    {
        // ============================================================
        // GET ALL - LISTADO PAGINADO (VERSION REFACTORIZADA)
        // ============================================================
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<PurchaseOrderSummaryDto>>>> GetAll([FromQuery] PurchaseOrderQueryParameters queryParams)
        {
            try
            {
                var query = context.PurchaseOrder
                    .AsNoTracking()
                    .Include(po => po.Purchase)
                    .Include(po => po.Quote)
                        .ThenInclude(q => q.Supplier)
                    .AsQueryable();

                // 1. FILTROS DE NEGOCIO (Fechas y Estado)
                if (!string.IsNullOrWhiteSpace(queryParams.Status))
                {
                    query = query.Where(po => po.Status.ToLower() == queryParams.Status.ToLower().Trim());
                }

                if (queryParams.StartDate.HasValue)
                {
                    query = query.Where(po => po.Date >= queryParams.StartDate.Value);
                }

                if (queryParams.EndDate.HasValue)
                {
                    var endDate = queryParams.EndDate.Value.AddDays(1).AddTicks(-1);
                    query = query.Where(po => po.Date <= endDate);
                }

                // 2. BÚSQUEDA DINÁMICA (DRY)
                // Nota: Para campos anidados como Purchase.ProjectName, ApplySearch 
                // requiere que el string sea exacto al nombre de la propiedad.
                query = query.ApplySearch(queryParams.Search, "OrderNumber", "Status");

                // 3. ORDENAMIENTO DINÁMICO (DRY)
                query = query.ApplySorting(queryParams.SortBy, "Date:desc");

                // 4. PAGINACIÓN GENÉRICA
                var pagedResult = await query.ToPagedResponseAsync(queryParams.PageNumber, queryParams.PageSize);

                // 5. MAPEO MANUAL A DTO (Items -> SummaryDto)
                var dtos = pagedResult.Items.Select(po => po.ToSummaryDto()).ToList();

                var response = new PagedResponse<PurchaseOrderSummaryDto>(
                    dtos, 
                    pagedResult.TotalItems, 
                    pagedResult.PageNumber, 
                    pagedResult.PageSize
                );

                return Ok(new ApiResponse<PagedResponse<PurchaseOrderSummaryDto>>(true, "Listado obtenido.", response));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>(false, "Error: " + ex.Message));
            }
        }

        // ============================================================
        // CREAR ORDEN DE COMPRA (Transaccional)
        // ============================================================
        [HttpPost]
        public async Task<ActionResult<ApiResponse<PurchaseOrderDetailDto>>> CreatePurchaseOrder([FromBody] CreatePurchaseOrderDto dto)
        {
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                // 1. Validaciones
                var purchase = await context.Purchase
                    .Include(p => p.PurchaseOrder)
                    .FirstOrDefaultAsync(p => p.Id == dto.PurchaseId);

                if (purchase == null) return NotFound(new ApiResponse<PurchaseOrderDetailDto>(false, "La compra no existe."));
                if (purchase.PurchaseOrder != null) return Conflict(new ApiResponse<PurchaseOrderDetailDto>(false, "OC ya existente."));

                var quote = await context.Quotes
                    .Include(q => q.QuoteItems)
                    .FirstOrDefaultAsync(q => q.Id == dto.QuoteId);

                if (quote == null || quote.PurchaseId != dto.PurchaseId)
                    return BadRequest(new ApiResponse<PurchaseOrderDetailDto>(false, "Cotización inválida para esta compra."));

                // 2. Generar Número de OC
                int count = await context.PurchaseOrder.CountAsync(po => po.Date.Year == DateTime.UtcNow.Year);
                string orderNumber = $"OC-{DateTime.UtcNow.Year}-{(count + 1):D4}";

                // 3. Mapear y Calcular Totales (Snapshot)
                var newOrder = dto.ToModelFromCreate(orderNumber);
                
                decimal subTotal = quote.QuoteItems?.Sum(item => (item.UnitPrice ?? 0) * item.Quantity) ?? 0;
                newOrder.SubTotal = subTotal;
                newOrder.TaxRate = 19m;
                newOrder.TaxAmount = subTotal * 0.19m;
                newOrder.TotalAmount = subTotal + newOrder.TaxAmount;

                // 4. Actualizar Estados
                quote.Status = "Aprobada";
                purchase.Status = "Orden Generada";
                purchase.UpdatedAt = DateTime.UtcNow;

                context.PurchaseOrder.Add(newOrder);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                // 5. Retorno con carga completa
                var completeOrder = await context.PurchaseOrder
                    .Include(po => po.Purchase)
                    .Include(po => po.Quote).ThenInclude(q => q.Supplier)
                    .Include(po => po.Quote).ThenInclude(q => q.QuoteItems)
                    .FirstAsync(po => po.Id == newOrder.Id);

                return CreatedAtAction(nameof(GetPurchaseOrderById), new { id = newOrder.Id }, 
                    new ApiResponse<PurchaseOrderDetailDto>(true, "Orden generada.", completeOrder.ToDetailDto()));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new ApiResponse<string>(false, "Error: " + ex.Message));
            }
        }

        // ============================================================
        // OBTENER POR ID (Detalle)
        // ============================================================
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<PurchaseOrderDetailDto>>> GetPurchaseOrderById(int id)
        {
            var purchaseOrder = await context.PurchaseOrder
                .AsNoTracking()
                .Include(po => po.Purchase)
                .Include(po => po.Quote).ThenInclude(q => q.Supplier)
                .Include(po => po.Quote).ThenInclude(q => q.QuoteItems)
                .FirstOrDefaultAsync(po => po.Id == id);

            if (purchaseOrder == null) return NotFound(new ApiResponse<string>(false, "No encontrada."));

            return Ok(new ApiResponse<PurchaseOrderDetailDto>(true, "OC obtenida.", purchaseOrder.ToDetailDto()));
        }
    }
}