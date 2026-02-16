namespace ByG_Backend.src.DTOs.Supplier
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