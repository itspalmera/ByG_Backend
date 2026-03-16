using System.ComponentModel.DataAnnotations;

namespace ByG_Backend.src.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos para el registro de nuevos proveedores.
    /// Define las validaciones estrictas para asegurar la integridad de la base de datos de contactos comerciales.
    /// </summary>
    /// <param name="Rut">Identificador tributario único (RUT) del proveedor.</param>
    /// <param name="BusinessName">Nombre legal o Razón Social de la empresa.</param>
    /// <param name="ContactName">Nombre de la persona de contacto principal (opcional).</param>
    /// <param name="Email">Dirección de correo para el envío automático de solicitudes y órdenes.</param>
    /// <param name="Phone">Número telefónico de contacto (opcional).</param>
    /// <param name="Address">Dirección física de la empresa (opcional).</param>
    /// <param name="City">Ciudad de operación (opcional).</param>
    /// <param name="ProductCategories">Descripción de rubros o tipos de productos que ofrece (opcional).</param>
    public record SupplierCreateDto(
        [Required(ErrorMessage = "El RUT es obligatorio.")]
        [StringLength(12, MinimumLength = 8, ErrorMessage = "El RUT debe tener entre 8 y 12 caracteres.")]
        string Rut,

        [Required(ErrorMessage = "La Razón Social es obligatoria.")]
        [StringLength(150, ErrorMessage = "La Razón Social no puede exceder los 150 caracteres.")]
        string BusinessName,

        [StringLength(100, ErrorMessage = "El Nombre de Contacto no puede exceder los 100 caracteres.")]
        string? ContactName,

        [Required(ErrorMessage = "El Correo Electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del Correo Electrónico no es válido.")]
        [StringLength(100, ErrorMessage = "El Correo Electrónico no puede exceder los 100 caracteres.")]
        string Email,

        [StringLength(20, ErrorMessage = "El Teléfono no puede exceder los 20 caracteres.")]
        string? Phone,

        [StringLength(200, ErrorMessage = "La Dirección no puede exceder los 200 caracteres.")]
        string? Address,

        [StringLength(100, ErrorMessage = "La Ciudad no puede exceder los 100 caracteres.")]
        string? City,

        [StringLength(500, ErrorMessage = "Las Categorías no pueden exceder los 500 caracteres.")]
        string? ProductCategories
    );
}