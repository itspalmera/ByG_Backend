namespace ByG_Backend.src.DTOs
{
    public record PurchaseDetailDto(
        int Id,
        string PurchaseNumber,
        string ProjectName,
        string Status,
        string RequestDate,
        string UpdatedAt,
        string Requester,
        string? Observations,
        
        // Lista completa de productos
        List<PurchaseItemDto> PurchaseItems,

        // Banderas de estado para la UI (Ayuda a la navegación en NextJS)
        RequestQuoteDto? RequestQuote,   // ¿Ya se pidieron cotizaciones?
        bool HasPurchaseOrder,   // ¿Ya se generó la OC final?
        int? PurchaseOrderId
    );
}