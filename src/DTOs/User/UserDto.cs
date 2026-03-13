using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos para la representación general de un usuario.
    /// Utilizado principalmente en vistas de administración, listados de personal y 
    /// perfiles de consulta pública dentro del sistema.
    /// </summary>
    public record UserDto
    {
        /// <summary>
        /// Nombres del usuario.
        /// </summary>
        public string FirstName { get; set; } = null!;

        /// <summary>
        /// Apellidos del usuario.
        /// </summary>
        public string LastName { get; set; } = null!;

        /// <summary>
        /// Correo electrónico institucional del usuario.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Rol actual asignado (ej: Admin, GestorCompras).
        /// </summary>
        public string Role { get; set; } = null!;

        /// <summary>
        /// Indica si el usuario tiene permiso actual para acceder a la plataforma.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Fecha en la que el usuario fue dado de alta en el sistema.
        /// </summary>
        public string Registered { get; set; } = null!;

        /// <summary>
        /// Fecha y hora del último inicio de sesión registrado.
        /// </summary>
        public string? LastAccess { get; set; }
    }
}