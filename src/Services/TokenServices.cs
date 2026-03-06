using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims; 
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ByG_Backend.src.Interfaces;
using ByG_Backend.src.Models;
using Microsoft.Extensions.Configuration;

namespace ByG_Backend.src.Services
{
    /// <summary>
    /// Servicio encargado de la generación y gestión de tokens de autenticación JWT.
    /// Implementa <see cref="ITokenServices"/> para proporcionar tokens firmados que contienen 
    /// la identidad del usuario y sus roles asignados.
    /// </summary>
    public class TokenService : ITokenServices
    {
        private readonly IConfiguration _config;
        private readonly SymmetricSecurityKey _key;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="TokenService"/>.
        /// Configura la clave de firma simétrica extrayéndola de los archivos de configuración.
        /// </summary>
        /// <param name="config">Interfaz para acceder a las configuraciones (appsettings.json).</param>
        /// <exception cref="ArgumentNullException">Se lanza si no se encuentra la clave 'JWT:SignInKey' en la configuración.</exception>
        public TokenService(IConfiguration config)
        {
            _config = config;
            var signingKey = _config["JWT:SignInKey"] ?? throw new ArgumentNullException("Key not found");
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        }

        /// <summary>
        /// Genera un token JWT para un usuario autenticado.
        /// </summary>
        /// <remarks>
        /// El token incluye:
        /// - NameIdentifier: ID único del usuario en el sistema.
        /// - Email: Correo electrónico registrado.
        /// - Roles: Lista de roles asignados al usuario para el control de acceso (RBAC).
        /// El token tiene una validez predeterminada de 3 días y utiliza el algoritmo HMAC SHA-512.
        /// </remarks>
        /// <param name="user">Objeto de modelo que representa al usuario.</param>
        /// <param name="roles">Lista de cadenas con los nombres de los roles del usuario.</param>
        /// <returns>Una cadena que representa el token JWT codificado.</returns>
        public string GenerateToken(User user, List<string> roles)
        {
            // 1. Definición de los Claims (declaraciones de identidad)
            var claims = new List<Claim>
            {
                // Se utiliza ClaimTypes.NameIdentifier para compatibilidad con el sistema de Claims de .NET
                new Claim(ClaimTypes.NameIdentifier, user.Id), 
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            };

            // 2. Inyección de roles en el token para autorización posterior
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            
            // 3. Configuración de credenciales de firma con algoritmo SHA-512
            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            // 4. Descripción del Token (Payload y Metadata)
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(3),
                SigningCredentials = creds,
                Issuer = _config["JWT:Issuer"],
                Audience = _config["JWT:Audience"]
            };

            // 5. Creación y serialización del token
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}