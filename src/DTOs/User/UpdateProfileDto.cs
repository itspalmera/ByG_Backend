using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos utilizado para actualizar la información de perfil del usuario.
    /// Diseñado para soportar actualizaciones parciales; los campos nulos serán ignorados durante el proceso de actualización.
    /// </summary>
    public record UpdateProfileDto
    {
        /// <summary>
        /// Nuevo nombre o nombres del usuario. 
        /// Si se omite, se mantendrá el valor actual en la base de datos.
        /// </summary>
        public string? FirstName { get; set; } 

        /// <summary>
        /// Nuevos apellidos del usuario.
        /// </summary>
        public string? LastName { get; set; } 

        /// <summary>
        /// Número telefónico de contacto actualizado.
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// Dirección de correo electrónico actualizada. 
        /// Nota: Cambiar este valor suele requerir una re-validación de identidad en el sistema.
        /// </summary>
        public string? Email { get; set; } 
    }
}