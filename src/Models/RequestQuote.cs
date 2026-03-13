using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.Models
{
    /// <summary>
    /// Representa una Solicitud de Cotización (RFQ) formal enviada a uno o más proveedores.
    /// Esta entidad actúa como el puente entre un requerimiento de compra interno 
    /// y el proceso de licitación o búsqueda de ofertas en el mercado.
    /// </summary>
    public class RequestQuote
    {
        /// <summary>
        /// Identificador único autoincremental de la solicitud de cotización.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Folio correlativo que identifica el documento (ej: RFQ-2026-001).
        /// Se utiliza para la trazabilidad administrativa y comunicación con proveedores.
        /// </summary>
        public string Number { get; set; } = null!;

        /// <summary>
        /// Estado actual de la solicitud (ej: "Borrador", "Enviada", "Cerrada").
        /// Define el comportamiento del documento en el flujo de trabajo.
        /// </summary>
        public required string Status { get; set; }

        /// <summary>
        /// Fecha y hora de creación del registro en el sistema.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Fecha y hora en la que se realizó el envío masivo de la solicitud a los proveedores.
        /// Puede ser nulo si la solicitud aún está en preparación.
        /// </summary>
        public DateTime? SentAt { get; set; }

        // =========================
        // RELACIONES
        // =========================

        /// <summary>
        /// Identificador de la compra (requerimiento interno) asociada.
        /// </summary>
        public int PurchaseId { get; set; }

        /// <summary>
        /// Propiedad de navegación hacia la compra de origen. 
        /// Representa una relación 1 a 1 donde la RFQ depende de una Purchase.
        /// </summary>
        public Purchase Purchase { get; set; } = null!;

        /// <summary>
        /// Lista de relaciones con los proveedores invitados a esta solicitud específica.
        /// Provee la trazabilidad de qué proveedores recibieron este folio de RFQ.
        /// </summary>
        public List<RequestQuoteSupplier> RequestQuoteSuppliers { get; set; } = new();
    }
}