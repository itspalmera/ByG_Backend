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

        // =================================================================================
        // 1. CREAR ORDEN (Formalización desde Quote Aceptada)
        // =================================================================================
        [HttpPost]
        public async Task<ActionResult<ApiResponse<PurchaseOrderDetailDto>>> CreatePurchaseOrder([FromBody] CreatePurchaseOrderDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // A. Validaciones
                var purchase = await _context.Purchase
                    .Include(p => p.PurchaseOrder)
                    .FirstOrDefaultAsync(p => p.Id == dto.PurchaseId);

                if (purchase == null) return NotFound(new ApiResponse<PurchaseOrderDetailDto>(false, "Compra no encontrada."));
                if (purchase.PurchaseOrder != null) return Conflict(new ApiResponse<PurchaseOrderDetailDto>(false, "Esta compra ya tiene una OC generada."));

                var quote = await _context.Quotes
                    .Include(q => q.QuoteItems)
                    .FirstOrDefaultAsync(q => q.Id == dto.QuoteId);

                if (quote == null) return NotFound(new ApiResponse<PurchaseOrderDetailDto>(false, "Cotización no encontrada."));
                if (quote.PurchaseId != dto.PurchaseId) return BadRequest(new ApiResponse<PurchaseOrderDetailDto>(false, "La cotización no coincide con la compra."));

                // B. Generar Folio
                int count = await _context.PurchaseOrder.CountAsync(po => po.Date.Year == DateTime.UtcNow.Year);
                string orderNumber = $"OC-{DateTime.UtcNow.Year}-{(count + 1):D4}";

                // C. Crear Entidad (Estado inicial: WaitingApproval)
                var newOrder = dto.ToModelFromCreate(orderNumber);

                // D. Cálculos Financieros
                decimal subTotal = 0;
                if (quote.QuoteItems != null)
                {
                    foreach (var item in quote.QuoteItems)
                    {
                        subTotal += (item.UnitPrice ?? 0) * item.Quantity;
                    }
                }
                newOrder.SubTotal = subTotal;
                
                // Base Imponible
                decimal taxableAmount = newOrder.SubTotal - newOrder.Discount + newOrder.FreightCharge;
                if (taxableAmount < 0) taxableAmount = 0;

                newOrder.TaxAmount = taxableAmount * (newOrder.TaxRate / 100m);
                newOrder.TotalAmount = taxableAmount + newOrder.TaxAmount;

                // E. Actualizar Estados
                quote.Status = "Aprobada"; 
                // La Compra pasa a "OC autorizada" (que es el estado intermedio donde la OC existe pero no se ha enviado)
                purchase.Status = PurchaseStatuses.OrderAuthorized; 
                purchase.UpdatedAt = DateTime.UtcNow;

                _context.PurchaseOrder.Add(newOrder);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return await GetPurchaseOrderById(newOrder.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new ApiResponse<PurchaseOrderDetailDto>(false, "Error al crear la OC: " + ex.Message));
            }
        }


        // =================================================================================
        // 2. EDITAR ORDEN (Solo si está en Esperando Aprobación)
        // =================================================================================
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<PurchaseOrderDetailDto>>> UpdatePurchaseOrder(int id, [FromBody] UpdatePurchaseOrderDto dto)
        {
            var order = await _context.PurchaseOrder
                .Include(po => po.Purchase)
                .Include(po => po.Quote).ThenInclude(q => q.Supplier)
                .Include(po => po.Quote).ThenInclude(q => q.QuoteItems)
                .FirstOrDefaultAsync(po => po.Id == id);

            if (order == null) return NotFound(new ApiResponse<PurchaseOrderDetailDto>(false, "Orden no encontrada."));

            // REGLA DE NEGOCIO CRÍTICA: Bloqueo de edición
            if (order.Status != PurchaseOrderStatuses.WaitingApproval)
            {
                return BadRequest(new ApiResponse<PurchaseOrderDetailDto>(
                    false, 
                    $"No se puede editar una orden en estado '{order.Status}'. Solo órdenes en '{PurchaseOrderStatuses.WaitingApproval}' son editables."
                ));
            }

            // Aplicar cambios
            order.UpdateModel(dto);

            // Recalcular montos si es necesario
            if (dto.Discount.HasValue || dto.FreightCharge.HasValue)
            {
                decimal taxableAmount = order.SubTotal - order.Discount + order.FreightCharge;
                if (taxableAmount < 0) taxableAmount = 0;

                order.TaxAmount = taxableAmount * (order.TaxRate / 100m);
                order.TotalAmount = taxableAmount + order.TaxAmount;
            }

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new ApiResponse<PurchaseOrderDetailDto>(true, "Orden actualizada exitosamente.", order.ToDetailDto()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<PurchaseOrderDetailDto>(false, "Error al actualizar: " + ex.Message));
            }
        }

        // =================================================================================
        // 3. CAMBIAR ESTADO (Aprobar / Cancelar)
        // =================================================================================
        [HttpPatch("{id}/status")]
        public async Task<ActionResult<ApiResponse<string>>> UpdateStatus(int id, [FromBody] string newStatus)
        {
            if (string.IsNullOrWhiteSpace(newStatus)) 
                return BadRequest(new ApiResponse<string>(false, "El estado es requerido."));

            // Validar que el estado sea válido
            var validStatuses = new[] { PurchaseOrderStatuses.Sent, PurchaseOrderStatuses.Cancelled };
            if (!validStatuses.Contains(newStatus))
            {
                return BadRequest(new ApiResponse<string>(false, "Estado inválido. Use 'Enviada' o 'Cancelada'."));
            }

            var order = await _context.PurchaseOrder
                .Include(po => po.Purchase)
                .FirstOrDefaultAsync(po => po.Id == id);

            if (order == null) return NotFound(new ApiResponse<string>(false, "Orden no encontrada."));

            // Lógica de transición
            if (newStatus == PurchaseOrderStatuses.Sent)
            {
                // APROBAR y ENVIAR
                if (order.Status != PurchaseOrderStatuses.WaitingApproval)
                     return BadRequest(new ApiResponse<string>(false, "Solo se pueden aprobar órdenes en estado de espera."));
                
                order.Status = PurchaseOrderStatuses.Sent;
                order.SignedAt = DateTime.UtcNow; // Se firma en este momento
                
                // La Compra también avanza
                if (order.Purchase != null)
                {
                    order.Purchase.Status = PurchaseStatuses.OrderSent; // "OC enviada"
                    order.Purchase.UpdatedAt = DateTime.UtcNow;
                }

                // TODO: Aquí iría la llamada al servicio de envío de correos (Resend)
            }
            else if (newStatus == PurchaseOrderStatuses.Cancelled)
            {
                // CANCELAR
                order.Status = PurchaseOrderStatuses.Cancelled;
                // Nota: Podrías revertir el estado de la compra o dejarla como Rechazada, depende del flujo.
                // Por ahora mantenemos la compra en "OC autorizada" pero la orden muere, o pasamos a Rechazada.
                if (order.Purchase != null)
                {
                     // Opcional: order.Purchase.Status = PurchaseStatuses.Rejected; 
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new ApiResponse<string>(true, $"Orden de Compra actualizada a: {newStatus}"));
        }

// =================================================================================
        // 4. OBTENER POR ID (Detalle completo)
        // =================================================================================
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<PurchaseOrderDetailDto>>> GetPurchaseOrderById(int id)
        {
            var purchaseOrder = await _context.PurchaseOrder
                .AsNoTracking()
                .Include(po => po.Purchase)
                .Include(po => po.Quote).ThenInclude(q => q.Supplier)
                .Include(po => po.Quote).ThenInclude(q => q.QuoteItems)
                .FirstOrDefaultAsync(po => po.Id == id);

            if (purchaseOrder == null)
            {
                return NotFound(new ApiResponse<PurchaseOrderDetailDto>(
                    success: false, 
                    message: "Error al buscar.", 
                    errors: [$"No se encontró una Orden de Compra con el ID {id}."]
                ));
            }

            return Ok(new ApiResponse<PurchaseOrderDetailDto>(
                success: true, 
                message: "Orden de Compra obtenida exitosamente.", 
                data: purchaseOrder.ToDetailDto()
            ));
        }

        // =================================================================================
        // 5. OBTENER LISTADO (Paginación, Filtros y Ordenamiento)
        // =================================================================================
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<PurchaseOrderSummaryDto>>>> GetAll([FromQuery] PurchaseOrderQueryParameters queryParams)
        {
            var query = _context.PurchaseOrder
                .AsNoTracking()
                .Include(po => po.Purchase)
                .Include(po => po.Quote).ThenInclude(q => q.Supplier)
                .AsQueryable();

            // --- Filtros ---
            if (!string.IsNullOrWhiteSpace(queryParams.Search))
            {
                var term = queryParams.Search.ToLower().Trim();
                query = query.Where(po => 
                    po.OrderNumber.ToLower().Contains(term) || 
                    (po.Purchase != null && po.Purchase.ProjectName.ToLower().Contains(term)) || 
                    (po.Quote != null && po.Quote.Supplier != null && po.Quote.Supplier.BusinessName.ToLower().Contains(term))
                );
            }

            if (!string.IsNullOrWhiteSpace(queryParams.Status))
            {
                var status = queryParams.Status.ToLower().Trim();
                query = query.Where(po => po.Status.ToLower() == status);
            }

            if (queryParams.StartDate.HasValue)
                query = query.Where(po => po.Date >= queryParams.StartDate.Value);

            if (queryParams.EndDate.HasValue)
            {
                var endDate = queryParams.EndDate.Value.AddDays(1).AddTicks(-1);
                query = query.Where(po => po.Date <= endDate);
            }

            // --- Ordenamiento ---
            query = queryParams.SortBy?.ToLower() switch
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

            // --- Paginación ---
            var totalItems = await query.CountAsync();
            var items = await query
                .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .ToListAsync();

            var dtos = items.Select(po => po.ToSummaryDto()).ToList();

            var pagedData = new PagedResponse<PurchaseOrderSummaryDto>(dtos, totalItems, queryParams.PageNumber, queryParams.PageSize);

            return Ok(new ApiResponse<PagedResponse<PurchaseOrderSummaryDto>>(
                true, "Listado obtenido.", pagedData));
        }

        // =================================================================================
        // 6. DESCARGA PDF (Placeholder para evitar errores 404 en frontend)
        // =================================================================================
        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> GetPdf(int id)
        {
            var exists = await _context.PurchaseOrder.AnyAsync(po => po.Id == id);
            if (!exists) return NotFound("Orden de compra no encontrada.");

            // TODO: Implementar QuestPDF aquí.
            // Por ahora retornamos un PDF vacío válido o un array vacío para no romper el front.
            return File(new byte[0], "application/pdf", $"OC-{id}.pdf");
        }
    }
}