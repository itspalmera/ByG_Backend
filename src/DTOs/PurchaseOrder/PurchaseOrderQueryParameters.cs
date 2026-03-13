using System;

namespace ByG_Backend.src.DTOs
{
    /// <summary>
    /// Parámetros de consulta diseñados para filtrar y paginar el listado de Órdenes de Compra.
    /// Facilita la implementación de búsquedas dinámicas y el control de carga de datos en el frontend.
    /// </summary>
    public class PurchaseOrderQueryParameters
    {
        /// <summary>
        /// Término de búsqueda global que permite filtrar por folios (OC/Solicitud), 
        /// nombre del proyecto o razón social del proveedor.
        /// </summary>
        public string? Search { get; set; }

        /// <summary>
        /// Filtro por estado específico del documento (ej: "Emitida", "Anulada", "Pendiente").
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Criterio de ordenamiento para los resultados (ej: "date_desc", "total_asc").
        /// </summary>
        public string? SortBy { get; set; }

        /// <summary>
        /// Fecha inicial para el filtrado por rango de creación de la orden.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Fecha final para el filtrado por rango de creación de la orden.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Número de página actual para la paginación de resultados. Por defecto es 1.
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Cantidad de registros a retornar por página. Por defecto son 10.
        /// </summary>
        public int PageSize { get; set; } = 10;
    }
}