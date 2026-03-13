using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.Models
{
    /// <summary>
    /// Entidad intermedia que representa la relación entre una Solicitud de Cotización (RFQ) y un Proveedor.
    /// Permite gestionar el envío masivo de una misma solicitud a múltiples proveedores de forma independiente.
    /// </summary>
    public class RequestQuoteSupplier
    {
        /// <summary>
        /// Identificador único de la relación.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Fecha y hora exacta en la que se despachó la notificación o correo electrónico a este proveedor específico.
        /// </summary>
        public DateTime SentAt { get; set; }

        // =========================
        // RELACIONES 
        // =========================

        /// <summary>
        /// Identificador del proveedor invitado.
        /// </summary>
        public int SupplierId { get; set; }

        /// <summary>
        /// Propiedad de navegación hacia el proveedor asociado.
        /// </summary>
        public Supplier Supplier { get; set; } = null!;

        /// <summary>
        /// Identificador de la solicitud de cotización (RFQ) de origen.
        /// </summary>
        public int RequestQuoteId { get; set; }

        /// <summary>
        /// Propiedad de navegación hacia la solicitud de cotización (RFQ) asociada.
        /// </summary>
        public RequestQuote RequestQuote { get; set; } = null!;
    }
}