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
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController( ILogger<AuthController> logger, UserManager<User> userManager, ITokenServices tokenService, RoleManager<IdentityRole> roleManager) : ControllerBase
    {
        private readonly ILogger<AuthController> _logger = logger;
        private readonly UserManager<User> _userManager = userManager;
        private readonly ITokenServices _tokenService = tokenService;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;

        
        //[Authorize(Roles = "Admin")]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto newUser)
        {
            try
            {
                // 1. Validar el modelo (Data Annotations de RegisterDto)
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

                // 2. Validar contraseñas
                if (string.IsNullOrWhiteSpace(newUser.Password) || string.IsNullOrWhiteSpace(newUser.ConfirmPassword))
                    return BadRequest(new ApiResponse<string>(false, "La contraseña y la confirmación son requeridas"));

                if (newUser.Password != newUser.ConfirmPassword)
                    return BadRequest(new ApiResponse<string>(false, "Las contraseñas no coinciden"));

                // 3. Verificar si el correo ya existe
                var existingUser = await _userManager.FindByEmailAsync(newUser.Email);
                if (existingUser != null)
                    return Conflict(new ApiResponse<string>(false, "Ya existe una cuenta con este correo electrónico"));

                var roleToAssign = string.IsNullOrWhiteSpace(newUser.Role) ? "User" : newUser.Role;
                var roleExists = await _roleManager.RoleExistsAsync(roleToAssign);

                if (!roleExists)
                {
                    return BadRequest(new ApiResponse<string>(false, $"El rol '{roleToAssign}' no existe en el sistema."));
                }

                // 4. Mapear DTO a Modelo de Usuario
                var user = UserMapper.RegisterToUser(newUser);

                // 5. Intentar crear el usuario en Identity
                var createUser = await _userManager.CreateAsync(user, newUser.Password);

                // 6. VERIFICAR ÉXITO DE CREACIÓN ANTES DE SEGUIR
                if (!createUser.Succeeded)
                {
                    var identityErrors = createUser.Errors.Select(e => e.Description).ToList();
                    return BadRequest(new ApiResponse<string>(
                        false,
                        "Error al crear el usuario",
                        null,
                        identityErrors));
                }

                // 7. ASIGNACIÓN DE ROL (Ahora que el usuario existe en la DB)
                // Si el DTO no trae rol, asignamos "User" por defecto
                var roleToAssigna = string.IsNullOrWhiteSpace(newUser.Role) ? "User" : newUser.Role;
                
                var roleResult = await _userManager.AddToRoleAsync(user, roleToAssigna);

                if (!roleResult.Succeeded)
                {
                    // Nota: El usuario ya se creó, pero el rol falló. 
                    // Podrías decidir si borrar al usuario o devolver un error específico.
                    var roleErrors = roleResult.Errors.Select(e => e.Description).ToList();
                    return StatusCode(500, new ApiResponse<string>(
                        false,
                        "Usuario creado, pero hubo un error al asignar el rol",
                        null,
                        roleErrors));
                }

                // 8. Respuesta exitosa
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

                // Sin UnitOfWork: buscar usuario por email directamente con Identity
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

                // Tomamos el primer rol (Identity devuelve lista, pero tu sistema parece usar uno solo)
                // Si no tiene rol en Identity, usamos "User" por defecto o la propiedad user.Role
                var currentRole = roles.FirstOrDefault() ?? user.Role ?? "User";

                // Pasamos el rol al mapper
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