using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.DTO
{
    /// <summary>
    /// Objeto de transferencia de datos para la solicitud inicial de recuperación de contraseña.
    /// Representa el primer paso del flujo de seguridad donde el usuario solicita un código de verificación.
    /// </summary>
    public record ForgotPasswordDto
    {
        /// <summary>
        /// Dirección de correo electrónico asociada a la cuenta que se desea recuperar.
        /// Se utiliza para validar la existencia del usuario y enviar el código de restablecimiento.
        /// </summary>
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        public string Email { get; set; } = null!;
    }
}