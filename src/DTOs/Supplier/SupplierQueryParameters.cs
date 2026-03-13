using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ByG_Backend.src.DTOs
{
    /// <summary>
    /// Parámetros de consulta para el listado de proveedores.
    /// Implementa IValidatableObject para asegurar la coherencia entre rangos de fechas 
    /// y soporta el binding de parámetros desde la URL ([FromQuery]).
    /// </summary>
    public class SupplierQueryParameters : IValidatableObject
    {
        /// <summary>
        /// Número de página solicitado (base 1).
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Cantidad de registros por página. Por defecto 15 para optimizar el renderizado de tablas.
        /// </summary>
        public int PageSize { get; set; } = 15;

        /// <summary>
        /// Término de búsqueda general que filtra por RUT, Nombre o Correo electrónico.
        /// </summary>
        [MaxLength(100, ErrorMessage = "El término de búsqueda no puede exceder los 100 caracteres.")]
        public string? Search { get; set; }

        /// <summary>
        /// Filtro de estado: true para activos, false para inactivos, null para obtener todos.
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// Filtro específico por categoría de producto o rubro (ej: "Construcción", "Minería").
        /// </summary>
        [MaxLength(100, ErrorMessage = "La categoría no puede exceder los 100 caracteres.")]
        public string? ProductCategory { get; set; }

        /// <summary>
        /// Límite inferior para filtrar por fecha de registro en el sistema.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Límite superior para filtrar por fecha de registro en el sistema.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Atributo por el cual se desea ordenar la respuesta (ej: "BusinessName", "RegisteredAt").
        /// </summary>
        public string? SortBy { get; set; }

        /// <summary>
        /// Realiza validaciones personalizadas, como verificar que el rango de fechas sea lógico.
        /// </summary>
        /// <param name="validationContext">Contexto de la validación actual.</param>
        /// <returns>Una colección de errores de validación si existen.</returns>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (StartDate.HasValue && EndDate.HasValue && StartDate.Value > EndDate.Value)
            {
                yield return new ValidationResult(
                    "La fecha de inicio (StartDate) no puede ser posterior a la fecha de fin (EndDate).",
                    new[] { nameof(StartDate), nameof(EndDate) }
                );
            }
        }
    }
}