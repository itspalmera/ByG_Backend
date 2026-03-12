using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ByG_Backend.src.Models;

namespace ByG_Backend.src.Interfaces
{
    /// <summary>
    /// Define el contrato para los servicios de gestión de tokens de seguridad.
    /// Su principal responsabilidad es la emisión de credenciales digitales (JWT) 
    /// para la autorización de usuarios en el sistema.
    /// </summary>
    public interface ITokenServices
    {
        /// <summary>
        /// Crea un token de acceso basado en la identidad del usuario y sus privilegios.
        /// </summary>
        /// <param name="user">La entidad de usuario para la cual se generará el token.</param>
        /// <param name="roles">Una lista de los roles asociados al usuario para ser incluidos como claims de autorización.</param>
        /// <returns>Una cadena de texto que contiene el token de seguridad codificado.</returns>
        string GenerateToken(User user, List<string> roles);
    }
}