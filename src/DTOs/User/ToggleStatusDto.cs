using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos utilizado para modificar el estado de habilitación de un usuario.
    /// Permite activar o desactivar el acceso al sistema sin eliminar la información histórica.
    /// </summary>
    public record ToggleStatusDto
    {
        /// <summary>
        /// Correo electrónico del usuario cuyo estado se desea modificar.
        /// Utilizado como identificador único para la búsqueda en el almacén de identidad.
        /// </summary>
        [Required(ErrorMessage = "El campo Email es obligatorio.")]
        [EmailAddress(ErrorMessage = "El campo Email debe ser una dirección de correo electrónico válida.")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// El nuevo estado que se asignará al usuario.
        /// - true: Usuario habilitado para iniciar sesión.
        /// - false: Acceso restringido (bloqueado).
        /// </summary>
        public bool Status { get; set; } = false;
    }
}