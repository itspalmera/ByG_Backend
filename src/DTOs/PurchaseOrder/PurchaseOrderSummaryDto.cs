namespace ByG_Backend.src.DTOs
{
    public record PurchaseOrderSummaryDto(
        int Id,
        string OrderNumber,      // Folio OC
        string PurchaseNumber,   // Folio Solicitud (Para referencia)
        string ProjectName,      // Nombre obra
        string SupplierName,     // Nombre Proveedor
        string Date,             // Fecha emisión
        decimal TotalAmount,     // Monto total
        string Status            // Estado
    );
}