using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ByG_Backend.src.DTOs;

    /// <summary>
    /// Objeto de parámetros de consulta para el filtrado, ordenamiento y paginación 
    /// de los requerimientos de compra.
    /// </summary>
    public class PurchaseQueryParameters : IValidatableObject
    {
        /// <summary>
        /// Número de la página a recuperar. Por defecto es 1.
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Cantidad de registros por página. Configurado en 15 para un renderizado óptimo en desktop.
        /// </summary>
        public int PageSize { get; set; } = 15;

        /// <summary>
        /// Término de búsqueda para filtrado parcial por Folio, Proyecto o Solicitante.
        /// </summary>
        [MaxLength(100, ErrorMessage = "El término de búsqueda no puede exceder los 100 caracteres.")]
        public string? Search { get; set; }

        /// <summary>
        /// Filtro de coincidencia exacta para el estado del requerimiento.
        /// </summary>
        [MaxLength(50, ErrorMessage = "El estado no puede exceder los 50 caracteres.")]
        public string? Status { get; set; }

        /// <summary>
        /// Fecha inicial para el rango de creación de la solicitud.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Fecha final para el rango de creación de la solicitud.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Campo y dirección por la cual se desea ordenar los resultados (ej: "date_desc").
        /// </summary>
        public string? SortBy { get; set; }

        /// <summary>
        /// Valida que el rango de fechas proporcionado sea lógicamente correcto.
        /// </summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (StartDate.HasValue && EndDate.HasValue && StartDate.Value > EndDate.Value)
            {
                yield return new ValidationResult(
                    "La fecha de inicio (StartDate) no puede ser posterior a la fecha de fin (EndDate).",
                    [nameof(StartDate), nameof(EndDate)]
                );
            }
        }
    }