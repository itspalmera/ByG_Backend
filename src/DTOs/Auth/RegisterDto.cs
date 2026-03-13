using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ByG_Backend.src.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos para el registro de nuevos usuarios.
    /// Define las reglas de validación de negocio y seguridad para la creación de cuentas.
    /// </summary>
    public record RegisterDto
    {
        /// <summary>
        /// Nombres del usuario. Requiere un mínimo de 3 caracteres para evitar registros ambiguos.
        /// </summary>
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener al menos 3 caracteres.")]
        public string FirstName { get; set; } = null!;

        /// <summary>
        /// Apellidos del usuario.
        /// </summary>
        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El apellido debe tener al menos 3 caracteres.")]
        public string LastName { get; set; } = null!;

        /// <summary>
        /// Correo electrónico que servirá como identificador único y medio de comunicación.
        /// </summary>
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Contraseña de la cuenta. Debe cumplir con una política de complejidad fuerte:
        /// Al menos 8 caracteres, incluyendo mayúsculas, minúsculas, números y símbolos.
        /// </summary>
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+={}\[\]|\\:;\""<>,.?/~`]).+$",
        ErrorMessage = "La contraseña debe tener al menos una letra mayúscula, una letra minúscula, un número y un carácter especial.")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Campo de confirmación para asegurar que el usuario ingresó la contraseña correctamente.
        /// Debe coincidir exactamente con el campo Password.
        /// </summary>
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+={}\[\]|\\:;\""<>,.?/~`]).+$",
        ErrorMessage = "La contraseña debe tener al menos una letra mayúscula, una letra minúscula, un número y un carácter especial.")]
        public string ConfirmPassword { get; set; } = string.Empty;
        
        /// <summary>
        /// Rol asignado al usuario (ej: "Admin", "User").
        /// </summary>
        public string Role { get; set; } = null!;

        /// <summary>
        /// Indica si la cuenta se crea en estado activo. Por defecto es true.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Marca de tiempo de la creación formateada para el cliente.
        /// </summary>
        public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("dd/MM/yyyy");

        /// <summary>
        /// Registro del último acceso, inicialmente nulo al momento del registro.
        /// </summary>
        public string? LastLoginAt { get; set; }
    }
}