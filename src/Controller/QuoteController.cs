using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ByG_Backend.src.Data;
using ByG_Backend.src.DTOs;
using ByG_Backend.src.Helpers;
using ByG_Backend.src.Mappers;
using ByG_Backend.src.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ByG_Backend.src.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuoteController(DataContext context) : ControllerBase
    {
        private readonly DataContext _context = context;


        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<QuoteDto>>>> GetAll(
            [FromQuery] string? status,
            [FromQuery] string? searchTerm,
            [FromQuery] string? orderBy,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10
        )
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var query = _context.Quotes.AsNoTracking()
                .Include(q => q.QuoteItems)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(q => q.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();

                query = query.Where(q =>
                    (q.Status != null && q.Status.ToLower().Contains(term)) ||
                    q.Number.ToString().Contains(term)
                );
            }

            // Orden por defecto: más recientes primero
            query = orderBy?.ToLower() switch
            {
                "number" => query.OrderBy(q => q.Number),
                "number_desc" => query.OrderByDescending(q => q.Number),
                "status" => query.OrderBy(q => q.Status),
                "status_desc" => query.OrderByDescending(q => q.Status),
                "price" => query.OrderBy(q => q.TotalPrice ?? 0),
                "price_desc" => query.OrderByDescending(q => q.TotalPrice ?? 0),
                _ => query.OrderByDescending(q => q.Date)
            };

            var total = await query.CountAsync();

            var quotes = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = quotes.Select(QuoteMapper.QuoteToQuoteDto).ToList();

            return Ok(new ApiResponse<IEnumerable<QuoteDto>>(
                true,
                $"Cotizaciones obtenidas correctamente. Total: {total}",
                dtos
            ));
        }



        [HttpPatch("status")]
        public async Task<ActionResult<ApiResponse<string>>> ToggleStatus([FromBody] QuoteToggleStatusDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<string>(
                    false,
                    "Datos inválidos.",
                    null,
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                ));
            }

            var quote = await _context.Quotes.FirstOrDefaultAsync(q => q.Id == dto.id);
            if (quote == null)
                return NotFound(new ApiResponse<string>(false, "Cotización no encontrada"));

            // Normaliza el status (opcional, recomendado)
            var normalized = dto.newStatus.Trim();

            // Validación de estados permitidos (ajusta a tus estados reales)
            var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Pendiente",
                "Aprobada",
                "Rechazada",
                "Cancelada"
            };

            if (!allowed.Contains(normalized))
            {
                return BadRequest(new ApiResponse<string>(
                    false,
                    "Estado inválido. Estados permitidos: Pendiente, Aprobada, Rechazada, Cancelada."
                ));
            }

            // Si no cambia, responde igual (opcional)
            if (string.Equals(quote.Status, normalized, StringComparison.OrdinalIgnoreCase))
            {
                return Ok(new ApiResponse<string>(true, $"La cotización ya está en estado '{quote.Status}'."));
            }

            quote.Status = normalized;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new ApiResponse<string>(
                    false,
                    "Error al actualizar el estado de la cotización"
                ));
            }

            return Ok(new ApiResponse<string>(true, $"Estado actualizado a '{quote.Status}' correctamente"));
        }
    }

}