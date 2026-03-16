using System.ComponentModel.DataAnnotations;

namespace ByG_Backend.src.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos para la creación de un nuevo Requerimiento de Compra.
    /// Centraliza la información del encabezado, el listado de materiales y la pre-selección de proveedores.
    /// </summary>
    /// <param name="PurchaseNumber">Folio administrativo asignado manualmente o por sistema externo.</param>
    /// <param name="ProjectName">Nombre de la obra o proyecto de destino para los materiales.</param>
    /// <param name="Requester">Nombre de la persona o departamento que origina la necesidad.</param>
    /// <param name="Observations">Notas aclaratorias o instrucciones especiales para el comprador.</param>
    /// <param name="Items">Listado obligatorio de productos o servicios solicitados.</param>
    /// <param name="InitialSupplierIds">Lista opcional de IDs de proveedores sugeridos para iniciar la licitación.</param>
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

        /// <summary>
        /// Permite vincular proveedores al proceso de cotización desde el momento de la creación.
        /// </summary>
        List<int>? InitialSupplierIds 
    );
}