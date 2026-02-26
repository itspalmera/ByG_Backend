namespace ByG_Backend.src.DTOs
{
    public record SupplierDetailDto(
        int Id,
        string Rut,
        string BusinessName,
        string? ContactName,
        string Email,
        string? Phone,
        string? Address,
        string? City,
        string? ProductCategories,
        string RegisteredAt,
        bool IsActive
    );
}