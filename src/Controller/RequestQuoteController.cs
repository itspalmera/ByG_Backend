using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ByG_Backend.src.Data;
using ByG_Backend.src.DTOs;
using ByG_Backend.src.Helpers;
using ByG_Backend.src.Services;
using ByG_Backend.src.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;

namespace ByG_Backend.src.Controller
{
    [ApiController]
    [Route("api/Request")]
    public class RequestQuoteController(
        ILogger<RequestQuoteController> logger, DataContext context) : ControllerBase
    {
        private readonly ILogger<RequestQuoteController> _logger = logger;
        private readonly DataContext _context = context;

        //[Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<RequestQuoteDto>>>> GetAll(
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

            var query = _context.RequestQuotes.AsNoTracking()
                .Include(q => q.RequestQuoteSuppliers)
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
                _ => query.OrderByDescending(q => q.CreatedAt)
            };

            var total = await query.CountAsync();

            var quotes = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = quotes.Select(RequestQuoteMapper.RequestQuoteToRequestQuoteDto).ToList();

            return Ok(new ApiResponse<IEnumerable<RequestQuoteDto>>(
                true,
                $"Solicitud de cotizaciones obtenidas correctamente. Total: {total}",
                dtos
            ));
        }

        // =========================
        // Create Quote (Admin)
        // =========================
        [HttpPost("create")]
        public byte[] GenerarCotizacionPdf([FromBody] GenerarPdfRequestDto request)
        {
            // 1. Instancias tu documento con los datos
            var documento = new QuoteServices(request.Compra, request.Solicitud);

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