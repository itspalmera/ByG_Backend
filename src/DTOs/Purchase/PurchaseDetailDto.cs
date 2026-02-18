namespace ByG_Backend.src.DTOs
{
    public record PurchaseDetailDto(
        int Id,
        string PurchaseNumber,
        string ProjectName,
        string Status,
        DateTime RequestDate,
        DateTime UpdatedAt,
        string Requester,
        string? Observations,
        
        // Lista completa de productos
        List<PurchaseItemDto> PurchaseItems,

        // Banderas de estado para la UI (Ayuda a la navegación en NextJS)
        bool HasRequestQuote,   // ¿Ya se pidieron cotizaciones?
        bool HasPurchaseOrder   // ¿Ya se generó la OC final?
    );
}