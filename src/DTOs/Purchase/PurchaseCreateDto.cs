using System.ComponentModel.DataAnnotations;

namespace ByG_Backend.src.DTOs.Purchase
{
    public record PurchaseCreateDto(
        [Required(ErrorMessage = "El Folio/Número de compra es obligatorio.")]
        string PurchaseNumber, 

        [Required(ErrorMessage = "El nombre del proyecto es obligatorio.")]
        string ProjectName,

        [Required(ErrorMessage = "El solicitante es obligatorio.")]
        string Requester,

        string? Observations,

        // Validamos que venga la lista de productos
        [Required(ErrorMessage = "La compra debe incluir productos.")]
        [MinLength(1, ErrorMessage = "Debe haber al menos un producto en la solicitud.")]
        List<PurchaseItemCreateDto> Items
    );
}