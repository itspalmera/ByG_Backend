using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ByG_Backend.src.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos utilizado por los administradores para modificar 
    /// el nivel de acceso de un usuario existente.
    /// </summary>
    public record ChangeRoleDto
    {
        /// <summary>
        /// Correo electrónico del usuario al que se le desea cambiar el rol.
        /// Actúa como el identificador único para localizar la cuenta en Identity.
        /// </summary>
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// El nuevo rol asignado. 
        /// Está restringido mediante una expresión regular a los roles válidos del sistema:
        /// - Admin: Acceso total.
        /// - User: Acceso básico.
        /// - GestorCompras: Manejo de solicitudes y proveedores.
        /// - AutorizadorCompras: Capacidad de firmar y generar OCs.
        /// </summary>
        [Required(ErrorMessage = "El nuevo rol es obligatorio.")]
        [RegularExpression("^(Admin|User|GestorCompras|AutorizadorCompras)$", 
            ErrorMessage = "El rol debe ser 'Admin', 'User', 'GestorCompras' o 'AutorizadorCompras'.")]
        public string NewRole { get; set; } = string.Empty;
    }
}