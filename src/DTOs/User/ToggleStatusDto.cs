using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.DTOs
{
    public record ToggleStatusDto
    {
        [Required]
        [EmailAddress (ErrorMessage = "El campo Email debe ser una dirección de correo electrónico válida.")]
        public string Email { get; set; } = string.Empty;

        public bool Status { get; set; } = false;

    }
}