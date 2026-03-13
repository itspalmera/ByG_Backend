using System.ComponentModel.DataAnnotations;

namespace ByG_Backend.src.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos para la actualización de proveedores existentes.
    /// Incluye todas las validaciones de integridad y permite modificar el estado operativo (IsActive).
    /// </summary>
    /// <param name="Rut">Rol Único Tributario corregido o mantenido.</param>
    /// <param name="BusinessName">Razón Social actualizada de la empresa.</param>
    /// <param name="ContactName">Nombre del contacto actualizado.</param>
    /// <param name="Email">Correo electrónico institucional para notificaciones.</param>
    /// <param name="Phone">Número telefónico de contacto.</param>
    /// <param name="Address">Dirección física actualizada.</param>
    /// <param name="City">Ciudad de operación.</param>
    /// <param name="ProductCategories">Rubros o categorías de productos suministrados.</param>
    /// <param name="IsActive">Define si el proveedor está habilitado para participar en nuevas licitaciones.</param>
    public record SupplierUpdateDto(
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
        string? ProductCategories,

        [Required(ErrorMessage = "El estado (Activo/Inactivo) es obligatorio.")]
        bool IsActive
    );
}