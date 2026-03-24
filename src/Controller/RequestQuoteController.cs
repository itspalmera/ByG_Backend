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
using QuestPDF.Fluent;

namespace ByG_Backend.src.Controller
{
    /// <summary>
    /// Controlador encargado de gestionar las Solicitudes de Cotización (Request Quotes).
    /// Proporciona funcionalidades para el listado, visualización, generación de archivos PDF 
    /// y el envío de estos documentos a múltiples proveedores vía correo electrónico.
    /// </summary>
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

        /// <summary>
        /// Obtiene un listado paginado de solicitudes de cotización con filtros aplicables.
        /// </summary>
        /// <param name="status">Filtro opcional por estado de la solicitud.</param>
        /// <param name="searchTerm">Término de búsqueda para filtrar resultados.</param>
        /// <param name="orderBy">Campo de ordenamiento dinámico.</param>
        /// <param name="purchaseId">Filtro opcional para obtener solicitudes de una compra específica.</param>
        /// <param name="pageNumber">Número de la página a recuperar.</param>
        /// <param name="pageSize">Tamaño de la página de resultados.</param>
        /// <returns>Respuesta paginada con DTOs de solicitudes de cotización.</returns>
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

                if (!string.IsNullOrWhiteSpace(purchaseId))
                {
                    query = query.Where(q => q.PurchaseId.ToString() == purchaseId);
                }

                query = query.ApplySearch(searchTerm, "Status");
                query = query.ApplySorting(orderBy, "CreatedAt");

                var pagedResult = await query.ToPagedResponseAsync(pageNumber, pageSize);

                var dtos = pagedResult.Items.Select(RequestQuoteMapper.RequestQuoteToRequestQuoteDto).ToList();

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

        /// <summary>
        /// Obtiene una solicitud de cotización específica por su ID.
        /// </summary>
        /// <param name="id">ID de la solicitud.</param>
        /// <returns>La solicitud encontrada incluyendo sus proveedores asociados.</returns>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<RequestQuote>>> GetById(int id)
        {
            var requestQuote = await _context.RequestQuotes
                .Include(q => q.RequestQuoteSuppliers)
                    .ThenInclude(rqs => rqs.Supplier)
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.Id == id);

            if (requestQuote == null)
                return NotFound(new ApiResponse<RequestQuote>(false, "Solicitud no encontrada"));

            return Ok(new ApiResponse<RequestQuote>(true, "Solicitud encontrada", requestQuote));
        }

        /// <summary>
        /// Genera el contenido binario de un PDF de cotización a partir de datos temporales (DTO).
        /// </summary>
        /// <param name="request">Datos estructurados de la compra y la solicitud.</param>
        /// <returns>Arreglo de bytes que representa el archivo PDF.</returns>
        [HttpPost("create")]
        public byte[] GenerarCotizacionPdf([FromBody] PdfRequestDto request)
        {
            var (compra, solicitud) = PdfMapper.MapDtoToModels(request);
            var documento = new QuoteServices(compra, solicitud, _company);
            return documento.GeneratePdf(); 
        }

        /// <summary>
        /// Genera y permite la descarga del archivo PDF de una solicitud registrada en la base de datos.
        /// </summary>
        /// <param name="id">ID de la solicitud de cotización.</param>
        /// <returns>Archivo PDF para descarga.</returns>
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

        /// <summary>
        /// Genera un PDF y lo envía concurrentemente a una lista de correos electrónicos.
        /// </summary>
        /// <remarks>
        /// Utiliza <see cref="IEmailService"/> para procesar los envíos en paralelo mediante Task.WhenAll.
        /// </remarks>
        /// <param name="request">DTO que contiene la lista de destinatarios y la información para el PDF.</param>
        /// <returns>Resumen del resultado del envío múltiple.</returns>
        [HttpPost("SendQuotePdf")]
        public async Task<IActionResult> SentQuoteMultiple([FromBody] SendPdfRequestDto request)
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
    

        /// <summary>
        /// Envía la solicitud de cotización (PDF) por correo a un proveedor específico.
        /// </summary>
        /// <param name="requestQuoteId">ID de la solicitud (RFQ).</param>
        /// <param name="supplierId">ID del proveedor destinatario.</param>
        /// <returns>Resultado de la operación.</returns>
        [HttpPost("{requestQuoteId}/send-to-supplier/{supplierId}")]
        public async Task<IActionResult> SendQuoteToSupplier(int requestQuoteId, int supplierId)
        {
            try
            {
                // 1. Obtener la solicitud, la compra y el proveedor
                var requestQuote = await _context.RequestQuotes
                    .Include(rq => rq.Purchase)
                        .ThenInclude(p => p.PurchaseItems) // Necesario para el PDF
                    .Include(rq => rq.RequestQuoteSuppliers)
                        .ThenInclude(rqs => rqs.Supplier)
                    .FirstOrDefaultAsync(rq => rq.Id == requestQuoteId);

                if (requestQuote == null || requestQuote.Purchase == null)
                    return NotFound(new ApiResponse<string>(false, "Solicitud o Compra no encontrada."));

                var supplierRelation = requestQuote.RequestQuoteSuppliers.FirstOrDefault(rqs => rqs.SupplierId == supplierId);
                
                if (supplierRelation == null || supplierRelation.Supplier == null)
                    return NotFound(new ApiResponse<string>(false, "El proveedor no está vinculado a esta solicitud."));

                var supplierEmail = supplierRelation.Supplier.Email;

                if (string.IsNullOrWhiteSpace(supplierEmail))
                    return BadRequest(new ApiResponse<string>(false, "El proveedor no tiene un correo electrónico registrado."));

                // 2. Generar el PDF
                var documento = new QuoteServices(requestQuote.Purchase, requestQuote, _company);
                byte[] pdfBytes = documento.GeneratePdf();
                string nombreArchivo = $"Solicitud_Cotizacion_{requestQuote.Number}.pdf";

                // 3. Enviar el correo usando IEmailService
                await _emailService.SendPdfDocumentAsync(supplierEmail, pdfBytes, nombreArchivo);

                // 4. Actualizar el "SentAt" en la tabla intermedia
                supplierRelation.SentAt = DateTime.UtcNow;

                // 5. Actualizar el estado de la Compra y de la Solicitud
                if (requestQuote.Purchase.Status == PurchaseStatuses.Received)
                {
                    requestQuote.Purchase.Status = PurchaseStatuses.QuoteSent;
                    requestQuote.Purchase.UpdatedAt = DateTime.UtcNow;
                }

                if (requestQuote.Status == "Pendiente")
                {
                    requestQuote.Status = "Enviada";
                }

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<string>(true, $"Cotización enviada exitosamente a {supplierEmail}."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar la solicitud de cotización al proveedor {SupplierId}", supplierId);
                return StatusCode(500, new ApiResponse<string>(false, "Ocurrió un error al intentar enviar el correo."));
            }
        }
    
    }
}