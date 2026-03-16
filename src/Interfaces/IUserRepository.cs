using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ByG_Backend.src.DTOs;
using Microsoft.AspNetCore.Identity;

namespace ByG_Backend.src.Interfaces
{
    /// <summary>
    /// Define el contrato para las operaciones de persistencia y lógica avanzada de usuarios.
    /// Actúa como una capa de abstracción sobre el almacenamiento de datos de identidad,
    /// permitiendo implementar reglas de negocio específicas para la gestión de cuentas.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Realiza el restablecimiento de la contraseña de un usuario validando los datos 
        /// proporcionados en el DTO de recuperación.
        /// </summary>
        /// <remarks>
        /// Este método debe integrar la validación de tokens o códigos manuales y la 
        /// interacción con el sistema de seguridad de ASP.NET Core Identity.
        /// </remarks>
        /// <param name="dto">DTO que contiene la información de identidad, código de verificación y nueva contraseña.</param>
        /// <returns>Una tarea que representa la operación asíncrona, cuyo resultado es un <see cref="IdentityResult"/>.</returns>
        Task<IdentityResult> ResetPasswordAsync(ResetPasswordDto dto);
    }
}