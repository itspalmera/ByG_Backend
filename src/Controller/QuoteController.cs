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
using ByG_Backend.src.Options;
using Microsoft.Extensions.Options;

namespace ByG_Backend.src.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuoteController(DataContext context, IOptions<CompanyInfoOptions> companyOptions) : ControllerBase
    {
        private readonly DataContext _context = context;
        private readonly CompanyInfoOptions _company = companyOptions.Value;



        // =========================
        // GET ALL WITH FILTERS, SEARCH, PAGINATION AND SORTING 
        // =========================
        
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<QuoteDto>>>> GetAll(
            [FromQuery] string? status,
            [FromQuery] string? searchTerm,
            [FromQuery] int? purchaseId,
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
                .Include(q => q.Supplier)
                .AsQueryable();

            
            if (purchaseId.HasValue)
            {
                query = query.Where(q => q.PurchaseId == purchaseId.Value);
            }
            
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
                return BadRequest(new ApiResponse<string>(false, "Datos inválidos.", null, ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
            }

            // 1. Buscamos la cotización y cargamos los datos necesarios para el PDF (Purchase y RequestQuote)
            var quote = await _context.Quotes
                .Include(q => q.Purchase)
                    .ThenInclude(p => p.PurchaseItems) // Necesario para la lista de items del PDF
                .Include(q => q.Purchase)
                    .ThenInclude(p => p.RequestQuote) // Necesario para el número de solicitud
                .FirstOrDefaultAsync(q => q.Id == dto.id);

            if (quote == null)
                return NotFound(new ApiResponse<string>(false, "Cotización no encontrada"));

            var normalized = dto.newStatus.Trim();
            var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Pendiente", "Aprobada", "Rechazada" };

            if (!allowed.Contains(normalized))
            {
                return BadRequest(new ApiResponse<string>(false, "Estado inválido."));
            }

            // Si el estado no cambia, salimos
            if (string.Equals(quote.Status, normalized, StringComparison.OrdinalIgnoreCase))
            {
                return Ok(new ApiResponse<string>(true, $"La cotización ya está en estado '{quote.Status}'."));
            }

            // ==============================================================================
            //  LÓGICA DE GENERACIÓN DE PDF AL APROBAR
            // ==============================================================================
            if (normalized.Equals("Aprobada", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    // Validamos que tengamos los datos necesarios
                    if (quote.Purchase != null && quote.Purchase.RequestQuote != null)
                    {
                        // 1. Instanciamos tu servicio de PDF con los datos reales de la compra
                        var pdfService = new QuoteServices(quote.Purchase, quote.Purchase.RequestQuote, _company);
                        
                        // 2. Generamos los bytes
                        byte[] pdfBytes = pdfService.GeneratePdf();

                        // 3. Definimos dónde guardar el registro (Ej: carpeta "Registros" en el servidor)
                        string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Registros", "Aprobadas");
                        
                        // Crear directorio si no existe
                        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                        // Nombre del archivo: Solicitud_NUMERO_IDCOTIZACION.pdf
                        string fileName = $"Registro_{quote.Purchase.RequestQuote.Number}_Cot{quote.Id}.pdf";
                        string fullPath = Path.Combine(folderPath, fileName);

                        // 4. Guardamos el archivo físicamente
                        await System.IO.File.WriteAllBytesAsync(fullPath, pdfBytes);

                        // Opcional: Podrías guardar la ruta del archivo en la base de datos si tuvieras un campo "PdfPath"
                        // quote.PdfPath = fullPath; 
                    }
                }
                catch (Exception ex)
                {
                    // Loguear el error pero no detener el flujo principal, o retornar error según prefieras
                    Console.WriteLine($"Error generando PDF de registro: {ex.Message}");
                    // return StatusCode(500, new ApiResponse<string>(false, "Error al generar el PDF de respaldo."));
                }
            }
            // ==============================================================================

            quote.Status = normalized;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new ApiResponse<string>(false, "Error al actualizar el estado en base de datos."));
            }

            return Ok(new ApiResponse<string>(true, $"Estado actualizado a '{quote.Status}' y registro generado correctamente."));
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

            
            QuoteMapper.UpdateQuoteFromDto(quote, dto);

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
        // Crear cotización (Admin)
        // =========================
        //[Authorize(Roles = "Admin")]
        [HttpPost("create")]
        public async Task<ActionResult<ApiResponse<QuoteDto>>> CreateQuote([FromBody] CreateQuoteDto dto)
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

            var quote = QuoteMapper.CreateQuoteFromDto(dto);

            _context.Quotes.Add(quote);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new ApiResponse<QuoteDto>(
                    false,
                    "Error al crear la cotización"
                ));
            }

            return Ok(new ApiResponse<QuoteDto>(
                true,
                "Cotización creada correctamente",
                QuoteMapper.QuoteToQuoteDto(quote)
            ));
        }

    
    
    }

}