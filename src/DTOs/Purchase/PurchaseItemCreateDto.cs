using System.ComponentModel.DataAnnotations;
namespace ByG_Backend.src.DTOs
{
// Para crear items cuando viene la solicitud desde el sistema externo
    public record PurchaseItemCreateDto(
        [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
        string Name,

        string? BrandModel,
        string? Description,

        [Required(ErrorMessage = "La unidad de medida es obligatoria.")]
        string Unit, // Ej: "Unidad", "Saco"

        string? Size,

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0.")]
        int Quantity
    );
}