using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.DTOs
{
    /// <summary>
    /// Representa los datos del proveedor formateados específicamente para su visualización en reportes PDF.
    /// Contiene la información legal y de contacto necesaria para el encabezado de documentos como Órdenes de Compra.
    /// </summary>
    public class PdfSupplierData
    {
        /// <summary>
        /// Nombre o Razón Social de la empresa proveedora.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Dirección física de la oficina o casa matriz del proveedor.
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// Rol Único Tributario (RUT) del proveedor, esencial para la validez tributaria del documento.
        /// </summary>
        public string? Rut { get; set; }

        /// <summary>
        /// Número telefónico de contacto de la empresa proveedora.
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// Nombre de la persona de contacto específica que gestionó la oferta o el pedido.
        /// </summary>
        public string? Contact { get; set; }
    }
}