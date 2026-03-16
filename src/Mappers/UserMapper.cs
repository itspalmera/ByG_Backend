using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ByG_Backend.src.DTOs;
using ByG_Backend.src.Models;

namespace ByG_Backend.src.Mappers
{
    /// <summary>
    /// Clase encargada de mapear la entidad de usuario Identity (<see cref="User"/>) 
    /// hacia sus diferentes representaciones de Objetos de Transferencia de Datos (DTOs).
    /// Centraliza la lógica de conversión para registro, visualización y autenticación.
    /// </summary>
    public class UserMapper
    {
        /// <summary>
        /// Mapea un DTO de registro a una nueva entidad de Usuario.
        /// </summary>
        /// <remarks>
        /// Se inicializa el UserName igual al Email, se establece la fecha de registro actual 
        /// y se marca la cuenta como activa por defecto.
        /// </remarks>
        /// <param name="dto">Datos de registro del nuevo usuario.</param>
        /// <returns>Entidad User lista para ser procesada por el UserManager.</returns>
        public static User RegisterToUser(RegisterDto dto) =>
            new()
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Role = dto.Role,
                Registered = DateOnly.FromDateTime(DateTime.UtcNow),
                IsActive = true,
            };

        /// <summary>
        /// Convierte una entidad User en un DTO de información general.
        /// </summary>
        /// <remarks>
        /// Formatea las fechas de registro y el último acceso para su consumo en tablas administrativas.
        /// </remarks>
        /// <param name="user">Entidad de usuario de la base de datos.</param>
        /// <returns>DTO con datos básicos y de acceso del usuario.</returns>
        public static UserDto UserToUserDto(User user) =>
        new()
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            IsActive = user.IsActive,
            Role = user.Role,
            Registered = user.Registered.ToString("dd/MM/yyyy"), 
            LastAccess = user.LastAccess.HasValue 
            ? user.LastAccess.Value.ToString("dd/MM/yyyy HH:mm:ss") 
            : "Sin acceso"
        };

        /// <summary>
        /// Mapeo simplificado para la respuesta inmediata tras la creación de un usuario.
        /// </summary>
        public static NewUserDto UserToNewUserDto(User user) =>
        new()
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email
        };

        /// <summary>
        /// Genera el DTO de respuesta para un inicio de sesión exitoso.
        /// </summary>
        /// <remarks>
        /// Incluye el token JWT generado y aplana la información necesaria para el estado global de la App.
        /// </remarks>
        /// <param name="user">Entidad del usuario autenticado.</param>
        /// <param name="token">Token JWT de acceso.</param>
        /// <param name="role">Rol principal asignado.</param>
        /// <returns>DTO completo con credenciales y perfil del usuario.</returns>
        public static AuthenticatedUserDto UserToAuthenticatedDto(User user, string token, string role) =>
        new()
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            Phone = user.PhoneNumber ?? string.Empty,
            Token = token,
            Role = role,
            IsActive = user.IsActive,
            Registered = user.Registered.ToString("dd/MM/yyyy"),
            LastAccess = user.LastAccess.HasValue 
            ? user.LastAccess.Value.ToString("dd/MM/yyyy 'T' HH:mm:ss") 
            : "Sin acceso"
        };

        /// <summary>
        /// Actualiza las propiedades de un usuario existente a partir de los datos de su perfil.
        /// </summary>
        /// <remarks>
        /// Aplica limpieza de espacios (Trim) y normalización de correo electrónico.
        /// Solo actualiza los campos que no sean nulos o espacios en blanco en el DTO.
        /// </remarks>
        /// <param name="user">Entidad cargada en memoria que será modificada.</param>
        /// <param name="dto">DTO con los nuevos datos del perfil.</param>
        public static void UpdateUserFromDto(User user, UpdateProfileDto dto)
        {
            if (!string.IsNullOrWhiteSpace(dto.FirstName))
                user.FirstName = dto.FirstName.Trim();

            if (!string.IsNullOrWhiteSpace(dto.LastName))
                user.LastName = dto.LastName.Trim();

            if (!string.IsNullOrWhiteSpace(dto.Email))
                user.Email = dto.Email.Trim().ToLower();

            if (!string.IsNullOrWhiteSpace(dto.Phone))
                user.PhoneNumber = dto.Phone.Trim();
        }
    }
}