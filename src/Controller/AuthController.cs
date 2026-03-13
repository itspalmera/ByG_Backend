using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ByG_Backend.src.Models;
using ByG_Backend.src.Services;
using ByG_Backend.src.Interfaces;
using ByG_Backend.src.DTOs;
using ByG_Backend.src.Helpers;
using ByG_Backend.src.Mappers;
using Microsoft.AspNetCore.Authorization;

namespace ByG_Backend.src.Controller
{
    /// <summary>
    /// Controlador encargado de gestionar los procesos de autenticación y registro de usuarios.
    /// Utiliza ASP.NET Core Identity para la gestión de cuentas, roles y seguridad.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(ILogger<AuthController> logger, UserManager<User> userManager, ITokenServices tokenService, RoleManager<IdentityRole> roleManager) : ControllerBase
    {
        private readonly ILogger<AuthController> _logger = logger;
        private readonly UserManager<User> _userManager = userManager;
        private readonly ITokenServices _tokenService = tokenService;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;

        /// <summary>
        /// Registra un nuevo usuario en el sistema.
        /// </summary>
        /// <remarks>
        /// El proceso incluye:
        /// 1. Validación de anotaciones de datos en el DTO.
        /// 2. Verificación de coincidencia de contraseñas.
        /// 3. Comprobación de existencia previa del correo y del rol solicitado.
        /// 4. Creación del usuario en la base de datos.
        /// 5. Asignación del rol especificado (o "User" por defecto).
        /// </remarks>
        /// <param name="newUser">Objeto DTO con los datos de registro (Email, Password, Role, etc.).</param>
        /// <returns>
        /// 200 (OK): Usuario creado con éxito y sus datos básicos.
        /// 400 (BadRequest): Datos inválidos, contraseñas no coincidentes o error en Identity.
        /// 409 (Conflict): El correo electrónico ya está en uso.
        /// 500 (InternalServerError): Errores durante la asignación del rol o excepciones generales.
        /// </returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto newUser)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new ApiResponse<string>(
                        false,
                        "Datos inválidos",
                        null,
                        errors));
                }

                if (string.IsNullOrWhiteSpace(newUser.Password) || string.IsNullOrWhiteSpace(newUser.ConfirmPassword))
                    return BadRequest(new ApiResponse<string>(false, "La contraseña y la confirmación son requeridas"));

                if (newUser.Password != newUser.ConfirmPassword)
                    return BadRequest(new ApiResponse<string>(false, "Las contraseñas no coinciden"));

                var existingUser = await _userManager.FindByEmailAsync(newUser.Email);
                if (existingUser != null)
                    return Conflict(new ApiResponse<string>(false, "Ya existe una cuenta con este correo electrónico"));

                var roleToAssign = string.IsNullOrWhiteSpace(newUser.Role) ? "User" : newUser.Role;
                var roleExists = await _roleManager.RoleExistsAsync(roleToAssign);

                if (!roleExists)
                {
                    return BadRequest(new ApiResponse<string>(false, $"El rol '{roleToAssign}' no existe en el sistema."));
                }

                var user = UserMapper.RegisterToUser(newUser);

                var createUser = await _userManager.CreateAsync(user, newUser.Password);

                if (!createUser.Succeeded)
                {
                    var identityErrors = createUser.Errors.Select(e => e.Description).ToList();
                    return BadRequest(new ApiResponse<string>(
                        false,
                        "Error al crear el usuario",
                        null,
                        identityErrors));
                }

                var roleToAssigna = string.IsNullOrWhiteSpace(newUser.Role) ? "User" : newUser.Role;
                
                var roleResult = await _userManager.AddToRoleAsync(user, roleToAssigna);

                if (!roleResult.Succeeded)
                {
                    var roleErrors = roleResult.Errors.Select(e => e.Description).ToList();
                    return StatusCode(500, new ApiResponse<string>(
                        false,
                        "Usuario creado, pero hubo un error al asignar el rol",
                        null,
                        roleErrors));
                }

                var userDto = UserMapper.UserToNewUserDto(user);
                return Ok(new ApiResponse<NewUserDto>(true, "Usuario registrado exitosamente", userDto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>(
                    false,
                    "Error interno del servidor",
                    null,
                    new List<string> { ex.Message }));
            }
        }



        /// <summary>
        /// Autentica a un usuario y genera un token JWT de acceso.
        /// </summary>
        /// <remarks>
        /// El proceso incluye:
        /// 1. Verificación del correo electrónico.
        /// 2. Comprobación de si la cuenta está activa (IsActive).
        /// 3. Validación de la contraseña.
        /// 4. Actualización del último acceso ajustado a la zona horaria de Chile.
        /// 5. Generación de Token JWT con los roles del usuario.
        /// </remarks>
        /// <param name="loginDto">DTO con las credenciales (Email y Password).</param>
        /// <returns>
        /// 200 (OK): Login exitoso, devuelve el token y datos del usuario.
        /// 401 (Unauthorized): Credenciales incorrectas o cuenta deshabilitada.
        /// 500 (InternalServerError): Excepciones generales durante el proceso.
        /// </returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new ApiResponse<string>(false, "Datos inválidos", null, errors));
                }

                var user = await _userManager.FindByEmailAsync(loginDto.Email);

                if (user == null)
                    return Unauthorized(new ApiResponse<string>(false, "Correo o contraseña inválidos"));

                if (!user.IsActive)
                    return Unauthorized(new ApiResponse<string>(false, "Tu cuenta está deshabilitada. Contacta al administrador."));

                var okPassword = await _userManager.CheckPasswordAsync(user, loginDto.Password);
                if (!okPassword)
                    return Unauthorized(new ApiResponse<string>(false, "Correo o contraseña inválidos"));

                TimeZoneInfo chileZone = TimeZoneInfo.FindSystemTimeZoneById("Chile/Continental");
                user.LastAccess = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, chileZone);
                
                var updateResult = await _userManager.UpdateAsync(user);

                var roles = await _userManager.GetRolesAsync(user);
                var token = _tokenService.GenerateToken(user, roles.ToList());

                var currentRole = roles.FirstOrDefault() ?? user.Role ?? "User";

                var userDto = UserMapper.UserToAuthenticatedDto(user, token, currentRole);

                return Ok(new ApiResponse<AuthenticatedUserDto>(true, "Login exitoso", userDto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>(
                    false,
                    "Error interno del servidor",
                    null,
                    new List<string> { ex.Message }));
            }
        }
    }
}