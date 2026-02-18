using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ByG_Backend.src.DTOs;
using ByG_Backend.src.Services;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;

namespace ByG_Backend.src.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequestQuoteController : ControllerBase
    {
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