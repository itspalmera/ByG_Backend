using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos que representa a un usuario autenticado exitosamente.
    /// Contiene la información de perfil necesaria para el frontend y el token de acceso.
    /// </summary>
    public record AuthenticatedUserDto
    {
        /// <summary>
        /// Nombres del usuario para visualización en el perfil.
        /// </summary>
        public string FirstName { get; set; } = null!;

        /// <summary>
        /// Apellidos del usuario.
        /// </summary>
        public string LastName { get; set; } = null!;

        /// <summary>
        /// Correo electrónico institucional.
        /// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// Número telefónico de contacto registrado.
        /// </summary>
        public string Phone { get; set; } = null!;

        /// <summary>
        /// Token de acceso (JWT) que debe incluirse en las cabeceras de peticiones autorizadas.
        /// </summary>
        public string Token { get; set; } = null!;

        /// <summary>
        /// Rol asignado que determina los permisos de navegación en el sistema.
        /// </summary>
        public string Role { get; set; } = null!;

        /// <summary>
        /// Fecha de registro formateada como cadena para visualización directa.
        /// </summary>
        public string Registered { get; set; } = null!;

        /// <summary>
        /// Fecha y hora del último acceso exitoso. Puede ser "Sin acceso" si es la primera vez.
        /// </summary>
        public string? LastAccess { get; set; }

        /// <summary>
        /// Estado de habilitación de la cuenta en el sistema.
        /// </summary>
        public bool IsActive { get; set; }
    }
}