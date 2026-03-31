using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos que vincula un proveedor con una solicitud de cotización específica.
    /// Se utiliza para gestionar y auditar la participación de terceros en los procesos de compra.
    /// </summary>
    public record RequestQuoteSupplierDto
    {
        /// <summary>
        /// Marca de tiempo que indica cuándo se le envió formalmente la invitación al proveedor.
        /// </summary>
        public string SentAt { get; set; } = string.Empty;

        /// <summary>
        /// Referencia al identificador de la Solicitud de Cotización (RFQ).
        /// </summary>
        public int RequestQuoteId { get; set; }

        /// <summary>
        /// Referencia al identificador del Proveedor invitado.
        /// </summary>
        public int SupplierId { get; set; }

        public string SupplierName { get; set; } = string.Empty;
        public string SupplierRut { get; set; } = string.Empty;
        public string SupplierEmail { get; set; } = string.Empty;
    }
}