using System.ComponentModel.DataAnnotations;

namespace ByG_Backend.src.DTOs
{
    public record PurchaseUpdateDto(
        [Required] string ProjectName,
        [Required] string Requester,
        string? Observations
        // No incluimos Status aquí, el cambio de estado debe ser por métodos dedicados (workflow)
    );
}