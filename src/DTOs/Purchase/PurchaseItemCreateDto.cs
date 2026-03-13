using System.ComponentModel.DataAnnotations;

namespace ByG_Backend.src.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos para la creación de ítems dentro de un requerimiento de compra.
    /// Define las especificaciones técnicas y cuantitativas de los materiales solicitados.
    /// </summary>
    /// <param name="Name">Nombre o categoría del producto (ej: Cemento, Casco de seguridad).</param>
    /// <param name="BrandModel">Marca o modelo específico requerido para asegurar compatibilidad técnica.</param>
    /// <param name="Description">Detalles adicionales, especificaciones técnicas o uso previsto.</param>
    /// <param name="Unit">Unidad de medida (ej: "Unidad", "Saco", "Metro", "Global").</param>
    /// <param name="Size">Talla, dimensiones o formato del producto.</param>
    /// <param name="Quantity">Cantidad requerida. Debe ser un número entero positivo.</param>
    public record PurchaseItemCreateDto(
        [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
        string Name,

        string? BrandModel,
        string? Description,

        [Required(ErrorMessage = "La unidad de medida es obligatoria.")]
        string Unit,

        string? Size,

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0.")]
        int Quantity
    );
}