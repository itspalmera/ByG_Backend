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
using QuestPDF.Fluent;

namespace ByG_Backend.src.Controller
{
    /// <summary>
    /// Controlador encargado de gestionar las Cotizaciones (Quotes).
    /// Permite el registro, actualización, filtrado y la generación automática de documentos PDF
    /// al momento de la aprobación de una cotización.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class QuoteController(DataContext context, IOptions<CompanyInfoOptions> companyOptions) : ControllerBase
    {
        private readonly DataContext _context = context;
        private readonly CompanyInfoOptions _company = companyOptions.Value;

        /// <summary>
        /// Obtiene un listado paginado de cotizaciones con filtros avanzados.
        /// </summary>
        /// <remarks>
        /// Soporta filtros por estado, término de búsqueda (Número, Proveedor, Observaciones) 
        /// y asociación a una solicitud de compra específica.
        /// </remarks>
        /// <param name="status">Estado de la cotización (ej. Pendiente, Aprobada).</param>
        /// <param name="searchTerm">Texto para buscar en número, nombre del proveedor u observaciones.</param>
        /// <param name="purchaseId">ID de la compra relacionada.</param>
        /// <param name="orderBy">Campo por el cual ordenar los resultados.</param>
        /// <param name="pageNumber">Número de página (mínimo 1).</param>
        /// <param name="pageSize">Cantidad de registros por página (máximo 100).</param>
        /// <returns>Respuesta paginada con objetos de tipo QuoteDto.</returns>
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
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 100) pageSize = 100;

                var query = _context.Quotes.AsNoTracking()
                    .Include(q => q.QuoteItems)
                    .Include(q => q.Supplier)
                    .Include(q => q.Purchase)
                    .AsQueryable();

                if (purchaseId.HasValue)
                {
                    query = query.Where(q => q.PurchaseId == purchaseId.Value);
                }

                if (!string.IsNullOrWhiteSpace(status))
                {
                    var cleanStatus = status.Trim().ToLower();
                    query = query.Where(q => q.Status.ToLower() == cleanStatus);
                }

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var term = searchTerm.Trim().ToLower();
                    query = query.Where(q => 
                        q.Number.ToLower().Contains(term) || 
                        (q.Supplier != null && q.Supplier.BusinessName.ToLower().Contains(term)) ||
                        (q.Observations != null && q.Observations.ToLower().Contains(term))
                    );
                }

                if (string.IsNullOrWhiteSpace(orderBy))
                {
                    query = query.OrderByDescending(q => q.Date);
                }
                else
                {
                    query = query.ApplySorting(orderBy, "Date");
                }

                var pagedResult = await query.ToPagedResponseAsync(pageNumber, pageSize);
                var dtos = pagedResult.Items.Select(QuoteMapper.QuoteToQuoteDto).ToList();

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

        /// <summary>
        /// Obtiene el detalle de una cotización específica por su ID.
        /// </summary>
        /// <param name="id">Identificador único de la cotización.</param>
        /// <returns>Modelo de la cotización con sus ítems y proveedor incluidos.</returns>
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

        /// <summary>
        /// Cambia el estado de una cotización y genera un archivo PDF si el estado es "Aprobada".
        /// </summary>
        /// <remarks>
        /// Si la cotización se marca como "Aprobada", se utiliza <see cref="QuoteServices"/> para generar 
        /// un documento PDF que se guarda físicamente en el servidor (wwwroot/Registros/Aprobadas).
        /// </remarks>
        /// <param name="dto">DTO con el ID de la cotización y el nuevo estado.</param>
        /// <returns>Mensaje de confirmación del cambio de estado.</returns>
        [HttpPatch("status")]
        public async Task<ActionResult<ApiResponse<string>>> ToggleStatus([FromBody] QuoteToggleStatusDto dto)
        {
            var quote = await _context.Quotes
                .Include(q => q.Purchase).ThenInclude(p => p.PurchaseItems)
                .Include(q => q.Purchase).ThenInclude(p => p.RequestQuote)
                .FirstOrDefaultAsync(q => q.Id == dto.id);

            if (quote == null) return NotFound(new ApiResponse<string>(false, "No encontrada"));

            var normalized = dto.newStatus.Trim();
            
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

        /// <summary>
        /// Actualiza los datos de una cotización existente.
        /// </summary>
        /// <remarks>
        /// Solo permite actualizaciones si la cotización no está en estado final (Aprobada o Rechazada).
        /// </remarks>
        /// <param name="dto">DTO con la información actualizada de la cotización.</param>
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

        /// <summary>
        /// Registra una nueva cotización en el sistema.
        /// </summary>
        /// <param name="dto">Datos de la nueva cotización.</param>
        /// <returns>La cotización creada mapeada a DTO.</returns>
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