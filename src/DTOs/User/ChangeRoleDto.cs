using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.DTOs
{
    public record ChangeRoleDto
    {
        [Required]
        [EmailAddress (ErrorMessage = "El correo electrónico no es válido.")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^(Admin|User|GestorCompras|AutorizadorCompras)$", ErrorMessage = "El rol debe ser 'Admin', 'User', 'GestorCompras' o 'AutorizadorCompras'.")]
        public string NewRole { get; set; } = string.Empty;
    }
}