using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ByG_Backend.src.DTOs;

    public class PurchaseQueryParameters : IValidatableObject
    {
        // 1. Búsqueda inteligente (Folio, Proyecto, Solicitante)
        [MaxLength(100, ErrorMessage = "El término de búsqueda no puede exceder los 100 caracteres.")]
        public string? Search { get; set; }

        // 2. Filtro exacto por Estado (Ej: "Esperando revisión")
        [MaxLength(50, ErrorMessage = "El estado no puede exceder los 50 caracteres.")]
        public string? Status { get; set; }

        // 3. Rango de Fechas (RequestDate)
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string? SortBy { get; set; }

        // 5. Validación de Rango de Fechas
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
