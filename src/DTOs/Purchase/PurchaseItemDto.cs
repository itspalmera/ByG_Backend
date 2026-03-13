using System.ComponentModel.DataAnnotations;

namespace ByG_Backend.src.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos para la representación detallada de un ítem de compra.
    /// Utilizado para listar los productos o servicios dentro de una solicitud existente.
    /// </summary>
    /// <param name="Id">Identificador único del ítem en la base de datos.</param>
    /// <param name="Name">Nombre genérico o categoría del producto.</param>
    /// <param name="BrandModel">Marca o modelo específico solicitado (opcional).</param>
    /// <param name="Description">Detalles técnicos o especificaciones adicionales.</param>
    /// <param name="Unit">Unidad de medida (ej: UN, KG, Global).</param>
    /// <param name="Size">Dimensiones o tallaje del ítem (opcional).</param>
    /// <param name="Quantity">Cantidad total solicitada.</param>
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