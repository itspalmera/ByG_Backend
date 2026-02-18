namespace ByG_Backend.src.DTOs
{
    public record SupplierSummaryDto(
        int Id,
        string Rut,
        string BusinessName,
        string Email,
        string? ProductCategories,
        bool IsActive
    );
}