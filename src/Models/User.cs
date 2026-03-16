using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace ByG_Backend.src.Models
{
    /// <summary>
    /// Representa la entidad de usuario dentro del sistema.
    /// Hereda de <see cref="IdentityUser"/> para incluir la gestión base de autenticación 
    /// (Email, PasswordHash, Phone, etc.) y extiende con atributos específicos del negocio.
    /// </summary>
    public class User : IdentityUser
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
        /// Nombre del rol asignado (ej: "Admin", "User"). 
        /// Se utiliza para facilitar la lectura del rol principal en el frontend.
        /// </summary>
        public string Role { get; set; } = null!;

        /// <summary>
        /// Indica si la cuenta del usuario está habilitada para acceder al sistema.
        /// Por defecto es true.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Fecha exacta en la que se realizó el registro del usuario.
        /// Se inicializa automáticamente con la fecha UTC actual.
        /// </summary>
        public DateOnly Registered { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

        /// <summary>
        /// Almacena la fecha y hora del último inicio de sesión exitoso.
        /// Puede ser nulo si el usuario nunca ha ingresado al sistema.
        /// </summary>
        public DateTime? LastAccess { get; set; }
    }
}