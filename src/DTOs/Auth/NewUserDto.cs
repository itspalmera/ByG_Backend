using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos que representa a un usuario recién creado.
    /// Se utiliza para confirmar el éxito de la operación de registro devolviendo 
    /// los datos básicos de identidad.
    /// </summary>
    public record NewUserDto
    {
        /// <summary>
        /// Nombres del usuario registrado.
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Correo electrónico institucional asignado a la nueva cuenta.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Apellidos del usuario registrado.
        /// </summary>
        public string LastName { get; set; } = string.Empty;
    }
}