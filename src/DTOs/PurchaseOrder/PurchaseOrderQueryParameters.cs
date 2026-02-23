namespace ByG_Backend.src.DTOs
{
    public class PurchaseOrderQueryParameters
    {
        public string? Search { get; set; }        // Búsqueda global
        public string? Status { get; set; }        // Filtro exacto de estado
        public string? SortBy { get; set; }        // Ordenamiento (ej: "amount_desc")
        public DateTime? StartDate { get; set; }   // Rango Fecha Inicio
        public DateTime? EndDate { get; set; }     // Rango Fecha Fin
        
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}