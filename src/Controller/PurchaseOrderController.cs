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
using ByG_Backend.src.Services;
using QuestPDF.Fluent;
using ByG_Backend.src.Options;
using Microsoft.Extensions.Options;

namespace ByG_Backend.src.Controller
{
    /// <summary>
    /// Controlador encargado de la gestión de Órdenes de Compra (Purchase Orders).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseOrderController(DataContext context, IOptions<CompanyInfoOptions> companyOptions) : ControllerBase
    {

        private readonly CompanyInfoOptions _company = companyOptions.Value;
        
        /// <summary>
        /// Obtiene un listado paginado de todas las órdenes de compra.
        /// </summary>
        /// <remarks>
        /// Incluye la información relacionada de la Solicitud de Compra y el Proveedor asociado a la cotización.
        /// Soporta búsqueda por número de orden y estado, además de ordenamiento dinámico.
        /// </remarks>
        /// <param name="queryParams">Parámetros de paginación, filtros de fecha/estado y términos de búsqueda.</param>
        /// <returns>Respuesta paginada con el resumen de las órdenes de compra.</returns>
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

                query = query.ApplySearch(queryParams.Search, "OrderNumber", "Status");

                query = query.ApplySorting(queryParams.SortBy, "Date:desc");

                var pagedResult = await query.ToPagedResponseAsync(queryParams.PageNumber, queryParams.PageSize);

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

        /// <summary>
        /// Crea una nueva Orden de Compra a partir de una Cotización seleccionada.
        /// </summary>
        /// <remarks>
        /// El proceso es transaccional y realiza lo siguiente:
        /// 1. Valida que la Solicitud de Compra no tenga ya una OC asignada.
        /// 2. Genera un número correlativo de OC basado en el año actual (ej. OC-2026-0001).
        /// 3. Calcula Subtotal, IVA (19%) y Total basado en los ítems de la cotización.
        /// 4. Actualiza el estado de la Cotización a "Aprobada" y de la Solicitud a "Orden Generada".
        /// </remarks>
        /// <param name="dto">DTO con el ID de la Solicitud y el ID de la Cotización aprobada.</param>
        /// <returns>El detalle de la Orden de Compra generada.</returns>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<PurchaseOrderDetailDto>>> CreatePurchaseOrder([FromBody] CreatePurchaseOrderDto dto)
        {
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
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

                int count = await context.PurchaseOrder.CountAsync(po => po.Date.Year == DateTime.UtcNow.Year);
                string orderNumber = $"OC-{DateTime.UtcNow.Year}-{(count + 1):D4}";

                var newOrder = dto.ToModelFromCreate(orderNumber);
                
                decimal subTotal = quote.QuoteItems?.Sum(item => (item.UnitPrice ?? 0) * item.Quantity) ?? 0;
                newOrder.SubTotal = subTotal;
                newOrder.TaxRate = 19m; 
                newOrder.TaxAmount = subTotal * 0.19m;
                newOrder.TotalAmount = subTotal + newOrder.TaxAmount;

                quote.Status = "Aprobada";
                purchase.Status = "Orden Generada";
                purchase.UpdatedAt = DateTime.UtcNow;

                context.PurchaseOrder.Add(newOrder);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();

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

        /// <summary>
        /// Obtiene el detalle de una Orden de Compra por su ID.
        /// </summary>
        /// <param name="id">ID único de la Orden de Compra.</param>
        /// <returns>Detalle completo incluyendo ítems y proveedor.</returns>
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


        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> GeneratePurchaseOrderPdf(int id)
        {
            var order = await context.PurchaseOrder
                .Include(o => o.Quote)
                    .ThenInclude(q => q.QuoteItems)
                .Include(o => o.Quote)
                    .ThenInclude(q => q.Supplier)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            var document = new PurchaseOrderServices(order, _company);

            var pdf = document.GeneratePdf();

            return File(pdf, "application/pdf", $"OC_{order.OrderNumber}.pdf");
        }
    }
}