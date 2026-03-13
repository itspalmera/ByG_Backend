using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos utilizado para filtrar búsquedas de usuarios.
    /// Permite realizar consultas dinámicas en el panel administrativo basándose en criterios parciales.
    /// </summary>
    public record UserSearchDto
    {
        /// <summary>
        /// Filtro por dirección de correo electrónico. 
        /// Puede ser nulo si el usuario prefiere buscar solo por nombre.
        /// </summary>
        public string? email { get; set; }

        /// <summary>
        /// Filtro por nombre o apellido del usuario. 
        /// Ideal para implementar búsquedas del tipo "contiene" en la base de datos.
        /// </summary>
        public string? name { get; set; }
    }
}