using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ByG_Backend.src.Models;

namespace ByG_Backend.src.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos para la visualización de una Cotización.
    /// Proporciona una vista consolidada de la oferta de un proveedor para su revisión y comparación.
    /// </summary>
    public record QuoteDto
    {
        /// <summary>
        /// Identificador único de la cotización en el sistema.
        /// </summary>
        public string id { get; set; } = null!;

        /// <summary>
        /// Número de folio asignado por el proveedor a su cotización.
        /// </summary>
        public string Number { get; set; } = null!;

        /// <summary>
        /// Estado actual de la cotización (ej: "Pendiente", "Aprobada", "Rechazada").
        /// </summary>
        public string Status { get; set; } = null!;

        /// <summary>
        /// Fecha de la cotización formateada para su visualización.
        /// </summary>
        public string Date { get; set; } = null!;

        /// <summary>
        /// Monto total de la cotización incluyendo todos sus ítems.
        /// </summary>
        public decimal? TotalPrice { get; set; }

        /// <summary>
        /// Comentarios o notas adicionales del proveedor o del gestor de compras.
        /// </summary>
        public string? Observations { get; set; }

        /// <summary>
        /// Nombre legible del proveedor para evitar búsquedas adicionales en el cliente.
        /// </summary>
        public String? SupplierName { get; set; }

        /// <summary>
        /// Detalle de los productos, cantidades y precios que componen la oferta.
        /// </summary>
        public List<QuoteItemDetailDto> Items { get; set; } = new();
    }
}