using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos para la solicitud de envío masivo de documentos RFQ.
    /// Vincula la información técnica del reporte con una lista de destinatarios para su distribución.
    /// </summary>
    public record SendPdfRequestDto
    {
        /// <summary>
        /// Lista de direcciones de correo electrónico de los proveedores a los cuales 
        /// se les enviará la solicitud de cotización adjunta.
        /// </summary>
        public List<string> Emails { get; set; } = new();

        /// <summary>
        /// Conjunto de datos (Compra y Solicitud) necesarios para generar el archivo PDF 
        /// "on-the-fly" antes de realizar el despacho por correo.
        /// </summary>
        public PdfRequestDto PdfData { get; set; } = null!;
    }
}