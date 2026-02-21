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
using ByG_Backend.src.Models;
using ByG_Backend.src.Options;
using Microsoft.Extensions.Options;

namespace ByG_Backend.src.Controller
{
    [ApiController]
    [Route("api/Request")]
    public class RequestQuoteController(
        ILogger<RequestQuoteController> logger, DataContext context, IOptions<CompanyInfoOptions> companyOptions) : ControllerBase
    {
        private readonly ILogger<RequestQuoteController> _logger = logger;
        private readonly DataContext _context = context;
        private readonly CompanyInfoOptions _company = companyOptions.Value;

        //[Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<RequestQuoteDto>>>> GetAll(
            [FromQuery] string? status,
            [FromQuery] string? searchTerm,
            [FromQuery] string? orderBy,
            [FromQuery] string? purchaseId = null,
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

            if (!string.IsNullOrWhiteSpace(purchaseId))
            {
                query = query.Where(q => q.PurchaseId.ToString() == purchaseId);
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



        //[Authorize(Roles = "Admin")] 
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<RequestQuote>>> GetById(int id)
        {
            var requestQuote = await _context.RequestQuotes
                .Include(q => q.RequestQuoteSuppliers)
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.Id == id);

            if (requestQuote == null)
            {
                return NotFound(new ApiResponse<RequestQuote>(
                    false,
                    "Solicitud de cotización no encontrada"
                ));
            }

            return Ok(new ApiResponse<RequestQuote>(
                true,
                "Solicitud de cotización encontrada",
                requestQuote
            ));
        }


        // =========================
        // Create Quote (Admin)
        // =========================
        [HttpPost("create")]
        public byte[] GenerarCotizacionPdf([FromBody] PdfRequestDto request)
        {
            // 1. Invocamos al mapper para obtener los modelos originales
            var (compra, solicitud) = PdfMapper.MapDtoToModels(request);

            // 2. Instancias tu documento con los modelos mapeados
            var documento = new QuoteServices(compra, solicitud, _company);

            // 3. Generas y retornas el PDF
            return documento.GeneratePdf(); 
        }
    }
}