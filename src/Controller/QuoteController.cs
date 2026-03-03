using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using ByG_Backend.src.Data;
using ByG_Backend.src.DTOs;
using ByG_Backend.src.Helpers;
using ByG_Backend.src.Mappers;
using ByG_Backend.src.Models;
using ByG_Backend.src.Services;
using ByG_Backend.src.Options;
using ByG_Backend.src.RequestHelpers;
using QuestPDF.Fluent; // Indispensable para la paginación

namespace ByG_Backend.src.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuoteController(DataContext context, IOptions<CompanyInfoOptions> companyOptions) : ControllerBase
    {
        private readonly DataContext _context = context;
        private readonly CompanyInfoOptions _company = companyOptions.Value;

        // ============================================================
        // GET ALL (Filtros + Paginación Genérica)
        // ============================================================
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<QuoteDto>>>> GetAll(
            [FromQuery] string? status,
            [FromQuery] string? searchTerm,
            [FromQuery] int? purchaseId,
            [FromQuery] string? orderBy,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10
        )
        {
            try
            {
                // 1. Configuración de Paginación
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 100) pageSize = 100;

                // 2. Consulta Base (Incluyendo Relaciones)
                var query = _context.Quotes.AsNoTracking()
                    .Include(q => q.QuoteItems)
                    .Include(q => q.Supplier)
                    .Include(q => q.Purchase)
                    .AsQueryable();

                // 3. FILTROS (Lógica de Negocio)

                // A. Filtro por Compra
                if (purchaseId.HasValue)
                {
                    query = query.Where(q => q.PurchaseId == purchaseId.Value);
                }

                // B. Filtro por Estado
                if (!string.IsNullOrWhiteSpace(status))
                {
                    var cleanStatus = status.Trim().ToLower();
                    query = query.Where(q => q.Status.ToLower() == cleanStatus);
                }

                // C. Búsqueda Global (Manual para incluir Proveedor)
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var term = searchTerm.Trim().ToLower();
                    query = query.Where(q => 
                        q.Number.ToLower().Contains(term) || 
                        (q.Supplier != null && q.Supplier.BusinessName.ToLower().Contains(term)) ||
                        (q.Observations != null && q.Observations.ToLower().Contains(term))
                    );
                }


                // 4. Ordenamiento
                // Por defecto ordenamos por fecha descendente (lo más nuevo arriba)
                if (string.IsNullOrWhiteSpace(orderBy))
                {
                    query = query.OrderByDescending(q => q.Date);
                }
                else
                {
                    query = query.ApplySorting(orderBy, "Date");
                }

                // 5. Paginación y Ejecución
                var pagedResult = await query.ToPagedResponseAsync(pageNumber, pageSize);

                // 6. Mapeo a DTOs
                var dtos = pagedResult.Items.Select(QuoteMapper.QuoteToQuoteDto).ToList();

                // 7. Respuesta
                var finalResponse = new PagedResponse<QuoteDto>(
                    dtos, 
                    pagedResult.TotalItems, 
                    pagedResult.PageNumber, 
                    pagedResult.PageSize
                );

                return Ok(new ApiResponse<PagedResponse<QuoteDto>>(true, "Cotizaciones obtenidas correctamente", finalResponse));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>(false, "Error interno: " + ex.Message));
            }
        }

        // ============================================================
        // GET BY ID
        // ============================================================
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<Quote>>> GetById(int id)
        {
            var quote = await _context.Quotes
                .Include(q => q.QuoteItems)
                .Include(q => q.Supplier)
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quote == null)
                return NotFound(new ApiResponse<Quote>(false, "Cotización no encontrada"));

            return Ok(new ApiResponse<Quote>(true, "Cotización encontrada", quote));
        }

        // ============================================================
        // TOGGLE STATUS & PDF GENERATION
        // ============================================================
        [HttpPatch("status")]
        public async Task<ActionResult<ApiResponse<string>>> ToggleStatus([FromBody] QuoteToggleStatusDto dto)
        {
            var quote = await _context.Quotes
                .Include(q => q.Purchase).ThenInclude(p => p.PurchaseItems)
                .Include(q => q.Purchase).ThenInclude(p => p.RequestQuote)
                .FirstOrDefaultAsync(q => q.Id == dto.id);

            if (quote == null) return NotFound(new ApiResponse<string>(false, "No encontrada"));

            var normalized = dto.newStatus.Trim();
            
            // Lógica de PDF al aprobar
            if (normalized.Equals("Aprobada", StringComparison.OrdinalIgnoreCase) && quote.Status != "Aprobada")
            {
                try
                {
                    if (quote.Purchase?.RequestQuote != null)
                    {
                        var pdfService = new QuoteServices(quote.Purchase, quote.Purchase.RequestQuote, _company);
                        byte[] pdfBytes = pdfService.GeneratePdf();

                        string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Registros", "Aprobadas");
                        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                        string fileName = $"Registro_{quote.Purchase.RequestQuote.Number}_Cot{quote.Id}.pdf";
                        await System.IO.File.WriteAllBytesAsync(Path.Combine(folderPath, fileName), pdfBytes);
                    }
                }
                catch (Exception ex) { Console.WriteLine($"Error PDF: {ex.Message}"); }
            }

            quote.Status = normalized;
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<string>(true, $"Estado actualizado a {normalized}"));
        }

        // ============================================================
        // UPDATE & CREATE
        // ============================================================
        [HttpPut("update")]
        public async Task<ActionResult<ApiResponse<QuoteDto>>> UpdateQuote([FromBody] UpdateQuoteDto dto)
        {
            var quote = await _context.Quotes.FirstOrDefaultAsync(q => q.Number == dto.Number);
            if (quote == null) return NotFound(new ApiResponse<QuoteDto>(false, "No encontrada"));

            if (quote.Status == "Aprobada" || quote.Status == "Rechazada")
                return BadRequest(new ApiResponse<QuoteDto>(false, "La cotización ya está cerrada"));

            QuoteMapper.UpdateQuoteFromDto(quote, dto);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<QuoteDto>(true, "Actualizada", QuoteMapper.QuoteToQuoteDto(quote)));
        }

        [HttpPost("create")]
        public async Task<ActionResult<ApiResponse<QuoteDto>>> CreateQuote([FromBody] CreateQuoteDto dto)
        {
            var quote = QuoteMapper.CreateQuoteFromDto(dto);
            _context.Quotes.Add(quote);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<QuoteDto>(true, "Creada", QuoteMapper.QuoteToQuoteDto(quote)));
        }
    }
}