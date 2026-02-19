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
        public async Task<ActionResult<ApiResponse<List<PurchaseSummaryDto>>>> GetPurchases([FromQuery] PurchaseQueryParameters queryParams)
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
            // Este suele ser un filtro de selección exacta (Dropdown en el frontend)
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

            // 5. ORDENAMIENTO DINÁMICO
            query = queryParams.SortBy?.ToLower() switch
            {
                "date_asc" => query.OrderBy(p => p.RequestDate),
                "date_desc" => query.OrderByDescending(p => p.RequestDate), // Default visual
                "project_asc" => query.OrderBy(p => p.ProjectName),
                "project_desc" => query.OrderByDescending(p => p.ProjectName),
                "status_asc" => query.OrderBy(p => p.Status),
                // Por defecto: Las más recientes primero
                _ => query.OrderByDescending(p => p.RequestDate) 
            };

            // 6. EJECUCIÓN Y PROYECCIÓN
            // IMPORTANTE: El Select va AL FINAL, después de filtrar y ordenar.
            var purchases = await query
                .Select(p => new PurchaseSummaryDto(
                    p.Id,
                    p.PurchaseNumber,
                    p.ProjectName,
                    p.Status,
                    p.RequestDate,
                    p.Requester,
                    // Subconsulta optimizada para contar items
                    p.PurchaseItems != null ? p.PurchaseItems.Count : 0 
                ))
                .ToListAsync();

            return Ok(new ApiResponse<List<PurchaseSummaryDto>>(
                success: true,
                message: "Listado de compras obtenido exitosamente.",
                data: purchases
            ));
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
            // 1. Validar Folio único
            var folioExists = await _context.Purchase
                .AsNoTracking()
                .AnyAsync(p => p.PurchaseNumber == dto.PurchaseNumber);

            if (folioExists)
            {
                return Conflict(new ApiResponse<PurchaseDetailDto>(
                    success: false, message: "Error al crear.", errors: ["El Folio de Compra ya está registrado."]
                ));
            }

            // 2. Mapear Compra y sus Productos simultáneamente
            var newPurchase = dto.ToModelFromCreate();

            // 3. Guardar todo en una sola transacción
            _context.Purchase.Add(newPurchase);
            await _context.SaveChangesAsync();

            // 4. Retornar el detalle (la respuesta tendrá las banderas falsas y la lista de ítems)
            return CreatedAtAction(
                actionName: nameof(GetPurchaseById),
                routeValues: new { id = newPurchase.Id },
                value: new ApiResponse<PurchaseDetailDto>(
                    success: true, message: "Compra creada exitosamente.", data: newPurchase.ToDetailDto()
                )
            );
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
    }
}