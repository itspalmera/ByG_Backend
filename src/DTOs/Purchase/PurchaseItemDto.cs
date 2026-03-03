
using System.ComponentModel.DataAnnotations;

namespace ByG_Backend.src.DTOs{
    // Para visualizar el ítem en el detalle de la compra
    public record PurchaseItemDto(
        int Id,
        string Name,
        string? BrandModel,
        string? Description,
        string Unit,
        string? Size,
        int Quantity
    );
}