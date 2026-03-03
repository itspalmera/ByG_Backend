using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using ByG_Backend.src.Data;
using ByG_Backend.src.DTOs;
using ByG_Backend.src.Helpers;
using ByG_Backend.src.Services;
using ByG_Backend.src.Mappers;
using ByG_Backend.src.Models;
using ByG_Backend.src.Options;
using ByG_Backend.src.Interfaces;
using ByG_Backend.src.RequestHelpers;
using QuestPDF.Fluent; // Indispensable para la paginación genérica

namespace ByG_Backend.src.Controller
{
    [ApiController]
    [Route("api/Request")]
    public class RequestQuoteController(
        ILogger<RequestQuoteController> logger, 
        DataContext context, 
        IOptions<CompanyInfoOptions> companyOptions, 
        IEmailService emailService) : ControllerBase
    {
        private readonly ILogger<RequestQuoteController> _logger = logger;
        private readonly DataContext _context = context;
        private readonly CompanyInfoOptions _company = companyOptions.Value;
        private readonly IEmailService _emailService = emailService;

        // ============================================================
        // GET ALL (Filtros + Paginación Genérica DRY)
        // ============================================================
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<RequestQuoteDto>>>> GetAll(
            [FromQuery] string? status,
            [FromQuery] string? searchTerm,
            [FromQuery] string? orderBy,
            [FromQuery] string? purchaseId = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10
        )
        {
            try
            {
                var query = _context.RequestQuotes.AsNoTracking()
                    .Include(q => q.RequestQuoteSuppliers)
                    .AsQueryable();

                // 1. Filtros


                if (!string.IsNullOrWhiteSpace(purchaseId))
                {
                    query = query.Where(q => q.PurchaseId.ToString() == purchaseId);
                }

                query = query.ApplySearch(searchTerm, "Status");

                // 2. Ordenamiento
                query = query.ApplySorting(orderBy, "CreatedAt");

                // 3. USO DE LA EXTENSIÓN GENÉRICA
                var pagedResult = await query.ToPagedResponseAsync(pageNumber, pageSize);

                // 4. Mapeo a DTOs
                var dtos = pagedResult.Items.Select(RequestQuoteMapper.RequestQuoteToRequestQuoteDto).ToList();

                // 5. Envolver en el formato final de PagedResponse de DTOs
                var finalResponse = new PagedResponse<RequestQuoteDto>(
                    dtos, 
                    pagedResult.TotalItems, 
                    pagedResult.PageNumber, 
                    pagedResult.PageSize
                );

                return Ok(new ApiResponse<PagedResponse<RequestQuoteDto>>(
                    true, 
                    "Solicitudes obtenidas correctamente", 
                    finalResponse
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener solicitudes de cotización");
                return StatusCode(500, new ApiResponse<string>(false, "Error interno del servidor"));
            }
        }

        // ============================================================
        // GET BY ID
        // ============================================================
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<RequestQuote>>> GetById(int id)
        {
            var requestQuote = await _context.RequestQuotes
                .Include(q => q.RequestQuoteSuppliers)
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.Id == id);

            if (requestQuote == null)
                return NotFound(new ApiResponse<RequestQuote>(false, "Solicitud no encontrada"));

            return Ok(new ApiResponse<RequestQuote>(true, "Solicitud encontrada", requestQuote));
        }

        // ============================================================
        // PDF & EMAIL SERVICES (Lógica de Negocio)
        // ============================================================

        [HttpPost("create")]
        public byte[] GenerarCotizacionPdf([FromBody] PdfRequestDto request)
        {
            var (compra, solicitud) = PdfMapper.MapDtoToModels(request);
            var documento = new QuoteServices(compra, solicitud, _company);
            return documento.GeneratePdf(); 
        }

        [HttpGet("{id:int}/pdf")]
        public async Task<IActionResult> DownloadRequestQuotePdf(int id)
        {
            var rq = await _context.RequestQuotes
                .Include(r => r.Purchase).ThenInclude(p => p.PurchaseItems)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rq == null) return NotFound(new ApiResponse<string>(false, "Solicitud no encontrada"));
            if (rq.Purchase == null) return BadRequest(new ApiResponse<string>(false, "La solicitud no tiene compra asociada"));

            var pdf = new QuoteServices(rq.Purchase, rq, _company).GeneratePdf();
            return File(pdf, "application/pdf", $"Solicitud_{rq.Number}.pdf");
        }

        [HttpPost("SendQuotePdf")]
        public async Task<IActionResult> EnviarCotizacionMultiple([FromBody] SendPdfRequestDto request)
        {
            if (request.Emails == null || !request.Emails.Any())
                return BadRequest("Debes proporcionar al menos un correo.");

            try 
            {
                var (compra, solicitud) = PdfMapper.MapDtoToModels(request.PdfData);
                var documento = new QuoteServices(compra, solicitud, _company);
                byte[] pdfBytes = documento.GeneratePdf();

                string nombreArchivo = $"Cotizacion_{request.PdfData.Compra.PurchaseNumber}.pdf";
                
                var envioTasks = request.Emails.Select(email => 
                    _emailService.SendPdfDocumentAsync(email, pdfBytes, nombreArchivo)
                );

                await Task.WhenAll(envioTasks);

                return Ok(new { Message = $"Enviado con éxito a {request.Emails.Count} destinatarios." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el envío múltiple de PDF");
                return StatusCode(500, $"Error en el proceso de envío: {ex.Message}");
            }
        }
    }
}