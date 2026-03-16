using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.DTOs
{
    /// <summary>
    /// DTO maestro para la generación de reportes de Solicitud de Cotización (RFQ).
    /// Agrupa la información de la compra y la metadata de la solicitud para su 
    /// representación final en formato PDF.
    /// </summary>
    public class PdfRequestDto
    {
        /// <summary>
        /// Datos detallados de la compra, incluyendo el proyecto, el solicitante 
        /// y el listado técnico de materiales requeridos.
        /// </summary>
        public PdfPurchaseData Compra { get; set; } = null!;

        /// <summary>
        /// Información administrativa de la solicitud, como el número de folio (RFQ) 
        /// y su estado actual en el sistema.
        /// </summary>
        public PdfRequestData Solicitud { get; set; } = null!;
    }
}