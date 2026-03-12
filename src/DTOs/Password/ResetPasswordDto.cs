using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos para la fase final del restablecimiento de contraseña.
    /// Consolida la verificación del código temporal y la actualización de la nueva credencial.
    /// </summary>
    public class ResetPasswordDto
    {
        /// <summary>
        /// Código de verificación de 6 dígitos enviado previamente por correo electrónico.
        /// </summary>
        [Required(ErrorMessage = "El código es obligatorio")]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Correo electrónico del usuario que solicita el cambio.
        /// </summary>
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo no válido")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Nueva contraseña que el usuario desea establecer. 
        /// Debe cumplir con el estándar de longitud mínima de 8 caracteres.
        /// </summary>
        [Required(ErrorMessage = "La nueva clave es obligatoria")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La clave debe tener al menos 8 caracteres")]
        public string NewPassword { get; set; } = string.Empty;

        /// <summary>
        /// Confirmación de la nueva contraseña. 
        /// Utiliza una validación comparativa para garantizar la paridad con 'NewPassword'.
        /// </summary>
        [Required(ErrorMessage = "Debe confirmar la contraseña")]
        [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}