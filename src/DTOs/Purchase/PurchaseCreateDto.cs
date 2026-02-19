using System.ComponentModel.DataAnnotations;

namespace ByG_Backend.src.DTOs
{
    public record PurchaseCreateDto(
        [Required(ErrorMessage = "El Folio/Número de compra es obligatorio.")]
        string PurchaseNumber, 

        [Required(ErrorMessage = "El nombre del proyecto es obligatorio.")]
        string ProjectName,

        [Required(ErrorMessage = "El solicitante es obligatorio.")]
        string Requester,

        string? Observations,

        [Required(ErrorMessage = "La compra debe incluir productos.")]
        [MinLength(1, ErrorMessage = "Debe haber al menos un producto en la solicitud.")]
        List<PurchaseItemCreateDto> Items,

        //Permite al usuario elegir proveedores fijos desde el inicio
        List<int>? InitialSupplierIds 
    );
}