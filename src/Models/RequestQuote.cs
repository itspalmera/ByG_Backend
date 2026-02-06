using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.Models
{
    /// <summary>
    /// Solicitud de cotización enviada a proveedores
    /// </summary>
    public class RequestQuote
    {
        public int Id { get; set; }

        // Folio de la solicitud (ej: RFQ-2026-001)
        public string Number { get; set; } = null!;

        // Estado de la solicitud
        public string Status { get; set; }

        // Cuándo se creó y envió
        public DateTime CreatedAt { get; set; }
        public DateTime? SentAt { get; set; }
    }
}