namespace ByG_Backend.src.DTOs.Purchase
{
    public record PurchaseSummaryDto(
        int Id,
        string PurchaseNumber, // Folio interno (Vital para buscar)
        string ProjectName,    // Para filtrar por obra
        string Status,         // "Solicitud enviada", "Orden emitida", etc. [cite: 686]
        DateTime RequestDate,  // Para ordenar por las más recientes
        string Requester,      // Quién lo pidió
        int ItemsCount         // Dato calculado: Cuántos productos trae esta solicitud
    );
}