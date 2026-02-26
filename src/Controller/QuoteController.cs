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
            [FromQuery] string? orderBy,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10
        )
        {
            try
            {
                var query = _context.Quotes.AsNoTracking()
                    .Include(q => q.QuoteItems)
                    .AsQueryable();

                // 1. Filtros
                if (!string.IsNullOrWhiteSpace(status))
                {
                    query = query.Where(q => q.Status == status);
                }

                query = query.ApplySearch(searchTerm, "Status");
                
                // 2. Ordenamiento
                query = query.ApplySorting(orderBy, "Date");

                // 3. USO DE EXTENSIÓN GENÉRICA
                var pagedResult = await query.ToPagedResponseAsync(pageNumber, pageSize);

                // 4. Mapeo a DTOs
                var dtos = pagedResult.Items.Select(QuoteMapper.QuoteToQuoteDto).ToList();

                // 5. Envolver resultado
                var finalResponse = new PagedResponse<QuoteDto>(
                    dtos, 
                    pagedResult.TotalItems, 
                    pagedResult.PageNumber, 
                    pagedResult.PageSize
                );

                return Ok(new ApiResponse<PagedResponse<QuoteDto>>(true, "Cotizaciones obtenidas", finalResponse));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>(false, "Error: " + ex.Message));
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