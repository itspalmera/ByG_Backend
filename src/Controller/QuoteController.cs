using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;

using ByG_Backend.src.Data;
using ByG_Backend.src.DTOs;
using ByG_Backend.src.Helpers;
using ByG_Backend.src.Mappers;
using ByG_Backend.src.Models;
using ByG_Backend.src.Services;

namespace ByG_Backend.src.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuoteController(DataContext context) : ControllerBase
    {
        private readonly DataContext _context = context;



        // =========================
        // GET ALL WITH FILTERS, SEARCH, PAGINATION AND SORTING 
        // =========================
        //[Authorize(Roles = "Admin")]
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



        // =========================
        // GET BY ID
        // =========================

        //[Authorize(Roles = "Admin")] 
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<Quote>>> GetById(int id)
        {
            var quote = await _context.Quotes
                .Include(q => q.QuoteItems) // opcional, si quieres traer los items
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quote == null)
            {
                return NotFound(new ApiResponse<Quote>(
                    false,
                    "Cotización no encontrada"
                ));
            }

            return Ok(new ApiResponse<Quote>(
                true,
                "Cotización encontrada",
                quote
            ));
        }


        // =========================
        // TOGGLE STATUS (Admin)
        // =========================

        //[Authorize(Roles = "Admin")]
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
                "Rechazada"
            };

            if (!allowed.Contains(normalized))
            {
                return BadRequest(new ApiResponse<string>(
                    false,
                    "Estado inválido. Estados permitidos: Pendiente, Aprobada, Rechazada."
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



        // =========================
        // UPDATE QUOTE (Admin)
        // =========================

        //[Authorize(Roles = "Admin")]
        [HttpPut("update")]
        public async Task<ActionResult<ApiResponse<QuoteDto>>> UpdateQuote([FromBody] UpdateQuoteDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<QuoteDto>(
                    false,
                    "Datos inválidos.",
                    null,
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                ));
            }

            var quote = await _context.Quotes.FirstOrDefaultAsync(q => q.Number == dto.Number);
            if (quote is null)
                return NotFound(new ApiResponse<QuoteDto>(false, "Cotización no encontrada"));
            

            if (quote.Status == "Aprobada" || quote.Status == "Rechazada")
            {
                return BadRequest(new ApiResponse<QuoteDto>(
                    false,
                    "No se puede actualizar una cotizacion con estado 'Aprobada' o 'Rechazada'."
                ));
            }

            
            QuoteMapper.UpdateQuoteFromDto(quote);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new ApiResponse<QuoteDto>(
                    false,
                    "Error al actualizar la cotización"
                ));
            }

            return Ok(new ApiResponse<QuoteDto>(
                true,
                "Cotización actualizada correctamente",
                QuoteMapper.QuoteToQuoteDto(quote)
            ));
        }




        // =========================
        // Create Quote (Admin)
        // =========================
        [HttpPost("create")]
        public byte[] GenerarCotizacionPdf(Purchase compra, RequestQuote solicitud)
        {
            // 1. Instancias tu documento con los datos
            var documento = new QuoteServices(compra, solicitud);

            // 2. Le dices a QuestPDF que genere el archivo. 
            // Él internamente se encargará de llamar a Compose() y GetMetadata().
            
            // Si lo quieres guardar como archivo físico:
            // documento.GeneratePdf("MiCotizacion.pdf");

            // Si lo quieres en memoria (arreglo de bytes) como hablamos antes:
            byte[] pdfBytes = documento.GeneratePdf(); 

            return pdfBytes;
        }
    
    }

}