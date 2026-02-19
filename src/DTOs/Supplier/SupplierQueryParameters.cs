
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ByG_Backend.src.DTOs.Supplier
{
    // Usamos 'class' en lugar de 'record' aquí porque el framework de .NET 
    // enlaza mejor los parámetros [FromQuery] a propiedades con getters y setters.
    // Implementamos IValidatableObject para validaciones condicionales o cruzadas
    public class SupplierQueryParameters : IValidatableObject
    {
        // Búsqueda general (RUT, Nombre, Correo)
        // Límite de seguridad: Nadie busca con más de 100 caracteres
        [MaxLength(100, ErrorMessage = "El término de búsqueda no puede exceder los 100 caracteres.")]
        public string? Search { get; set; }

        // Filtro por Estado (True = Activos, False = Inactivos, Null = Todos)
        // Los booleanos no necesitan DataAnnotations, el Model Binder de .NET ya valida que sea true/false
        public bool? IsActive { get; set; }

        // Filtro por Categoría (Ej: "EPP", "Construcción")
        // 2. Limite lógico para categorías
        [MaxLength(100, ErrorMessage = "La categoría no puede exceder los 100 caracteres.")]
        public string? ProductCategory { get; set; }

        // Rango de Fechas de Registro
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // Ordenamiento dinámico
        public string? SortBy { get; set; }

        // 4. Lógica de validación avanzada (Fechas congruentes)
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (StartDate.HasValue && EndDate.HasValue && StartDate.Value > EndDate.Value)
            {
                // Si la fecha de inicio es mayor a la de fin, devolvemos un error automático 400 Bad Request
                yield return new ValidationResult(
                    "La fecha de inicio (StartDate) no puede ser posterior a la fecha de fin (EndDate).",
                    [nameof(StartDate), nameof(EndDate)] // Indicamos qué campos fallaron
                );
            }
        }
    }
}