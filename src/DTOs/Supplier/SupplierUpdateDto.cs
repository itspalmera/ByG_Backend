using System.ComponentModel.DataAnnotations;

namespace ByG_Backend.src.DTOs.Supplier
{
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