using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.DTOs
{
    public class ResetPasswordDto
    {
        [Required]
        public string Code { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La clave debe tener al menos 8 caracteres")]
        public string NewPassword { get; set; } = string.Empty;


        [Required]
        [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}