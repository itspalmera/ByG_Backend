using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ByG_Backend.src.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos utilizado para las solicitudes de inicio de sesión.
    /// Define las reglas de validación necesarias para autenticar a un usuario en el sistema.
    /// </summary>
    public record LoginDto
    {
        /// <summary>
        /// Dirección de correo electrónico del usuario.
        /// Debe ser una dirección válida y es obligatoria para el proceso.
        /// </summary>
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El email no es válido")]
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// Contraseña asociada a la cuenta del usuario.
        /// Se requiere obligatoriamente para la validación de credenciales.
        /// </summary>
        [Required(ErrorMessage = "La contraseña es requerida")]
        public string Password { get; set; } = string.Empty;
    }
}