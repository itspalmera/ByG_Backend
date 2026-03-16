using System;
using System.Collections.Generic;
using System.Linq;

namespace ByG_Backend.src.RequestHelpers
{
    /// <summary>
    /// Clase genérica para envolver las respuestas de la API que requieren paginación.
    /// Proporciona tanto la lista de elementos como los metadatos necesarios para que el frontend 
    /// pueda renderizar controles de navegación (páginas totales, cantidad de registros, etc.).
    /// </summary>
    /// <typeparam name="T">El tipo de objeto que contiene la lista (ej: PurchaseDto, UserDto).</typeparam>
    /// <param name="items">Lista de elementos de la página actual.</param>
    /// <param name="totalItems">Cantidad total de registros existentes en la base de datos para la consulta.</param>
    /// <param name="pageNumber">Número de la página actual.</param>
    /// <param name="pageSize">Cantidad de registros por página.</param>
    public class PagedResponse<T>(List<T> items, int totalItems, int pageNumber, int pageSize)
    {
        /// <summary>
        /// Colección de elementos correspondientes al segmento de datos solicitado.
        /// </summary>
        public List<T> Items { get; set; } = items;

        /// <summary>
        /// El número de página actual (índice basado en 1).
        /// </summary>
        public int PageNumber { get; set; } = pageNumber;

        /// <summary>
        /// La cantidad de elementos máxima permitida por página.
        /// </summary>
        public int PageSize { get; set; } = pageSize;

        /// <summary>
        /// El total de registros que coinciden con los filtros aplicados (sin paginar).
        /// </summary>
        public int TotalItems { get; set; } = totalItems;

        /// <summary>
        /// Cálculo dinámico de la cantidad total de páginas basado en el total de ítems y el tamaño de página.
        /// </summary>
        /// <remarks>
        /// Se utiliza <see cref="Math.Ceiling"/> para asegurar que cualquier residuo de elementos genere una página adicional.
        /// </remarks>
        public int TotalPages => (int)Math.Ceiling(totalItems / (double)pageSize);
    }
}