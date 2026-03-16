using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos utilizado para la transición de estados de una cotización.
    /// Facilita el flujo de aprobación permitiendo cambiar el estado de la oferta de manera controlada.
    /// </summary>
    public record QuoteToggleStatusDto
    {
        /// <summary>
        /// Identificador único de la cotización que se desea actualizar.
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// El nuevo estado que se asignará a la cotización (ej: "Aprobada", "Rechazada").
        /// Este valor es obligatorio para procesar el cambio en el flujo de trabajo.
        /// </summary>
        [Required(ErrorMessage = "El nuevo estado es obligatorio para realizar la transición.")]
        public string newStatus { get; set; } = string.Empty;
    }
}