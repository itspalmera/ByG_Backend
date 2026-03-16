using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.DTOs
{
    /// <summary>
    /// Modelo de datos simplificado para la representación de la solicitud (RFQ) en documentos PDF.
    /// Proporciona la información de identificación y estado necesaria para el encabezado del reporte.
    /// </summary>
    public class PdfRequestData
    {
        /// <summary>
        /// Identificador único de la solicitud de cotización en la base de datos.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Número de folio o correlativo de la solicitud (ej: RFQ-2026-001).
        /// Es el dato clave para que el proveedor referencie su oferta.
        /// </summary>
        public string Number { get; set; } = null!;

        /// <summary>
        /// Estado actual de la solicitud al momento de generar el documento (ej: "Pendiente", "Aprovada").
        /// </summary>
        public string Status { get; set; } = null!;
    }
}