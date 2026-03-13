using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos utilizado para la actualización de cotizaciones existentes.
    /// Soporta actualizaciones parciales, donde solo los campos proporcionados serán modificados.
    /// </summary>
    public record UpdateQuoteDto
    {
        /// <summary>
        /// Nuevo número de folio de la cotización, si requiere corrección.
        /// </summary>
        public string? Number { get; set; }

        /// <summary>
        /// Estado actualizado de la cotización (ej: "Pendiente", "En Revisión").
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Fecha de la cotización en formato cadena (ej: "2026-03-12").
        /// </summary>
        public string? Date { get; set; }

        /// <summary>
        /// Ajuste del monto total de la oferta si hubo cambios en los precios unitarios.
        /// </summary>
        public decimal? TotalPrice { get; set; }

        /// <summary>
        /// Colección de identificadores o nombres de ítems asociados a la cotización.
        /// </summary>
        public string[]? Items { get; set; }
    }
}