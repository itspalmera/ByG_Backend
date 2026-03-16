using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ByG_Backend.src.Data;
using ByG_Backend.src.DTOs;
using ByG_Backend.src.Helpers;
using ByG_Backend.src.Mappers;
using ByG_Backend.src.Models;
using ByG_Backend.src.Interfaces;
using ByG_Backend.src.RequestHelpers;
using ByG_Backend.src.DTO;

namespace ByG_Backend.src.Controller
{
    /// <summary>
    /// Controlador para la gestión de usuarios y perfiles.
    /// Proporciona funcionalidades administrativas (listado, cambio de roles, estado) 
    /// y de usuario (actualización de perfil, recuperación de contraseña).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UserController(
        ILogger<UserController> logger, 
        DataContext context, 
        UserManager<User> userManager, 
        IUserRepository repository, 
        IEmailService emailService) : ControllerBase
    {
        private readonly ILogger<UserController> _logger = logger;
        private readonly DataContext _context = context;
        private readonly UserManager<User> _userManager = userManager;
        private readonly IEmailService _emailService = emailService;
        private readonly IUserRepository _repository = repository;

        /// <summary>
        /// Obtiene un listado paginado de todos los usuarios registrados (Solo Administradores).
        /// </summary>
        /// <param name="isActive">Filtro opcional por estado de cuenta.</param>
        /// <param name="role">Filtro opcional por rol asignado.</param>
        /// <param name="searchTerm">Término de búsqueda global (Email, UserName, Nombres).</param>
        /// <param name="registeredFrom">Filtro de fecha desde (registro).</param>
        /// <param name="registeredTo">Filtro de fecha hasta (registro).</param>
        /// <param name="orderBy">Parámetro de ordenamiento dinámico.</param>
        /// <param name="pageNumber">Número de página.</param>
        /// <param name="pageSize">Registros por página.</param>
        /// <returns>Respuesta paginada con DTOs de usuario.</returns>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<UserDto>>>> GetAll(
            [FromQuery] bool? isActive,
            [FromQuery] string? role,
            [FromQuery] string? searchTerm,
            [FromQuery] DateOnly? registeredFrom,
            [FromQuery] DateOnly? registeredTo,
            [FromQuery] string? orderBy,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10
        )
        {
            try 
            {
                var query = _context.Users.AsNoTracking().AsQueryable();

                if (isActive.HasValue)
                    query = query.Where(u => u.IsActive == isActive.Value);

                if (!string.IsNullOrEmpty(role))
                    query = query.Where(u => u.Role == role);

                if (registeredFrom.HasValue)
                    query = query.Where(u => u.Registered >= registeredFrom.Value);

                if (registeredTo.HasValue)
                    query = query.Where(u => u.Registered <= registeredTo.Value);

                query = query.ApplySearch(searchTerm, "Email", "UserName", "FirstName", "LastName");

                query = query.ApplySorting(orderBy, "Registered");

                var pagedResult = await query.ToPagedResponseAsync(pageNumber, pageSize);

                var dtos = pagedResult.Items.Select(UserMapper.UserToUserDto).ToList();
                var finalPagedData = new PagedResponse<UserDto>(
                    dtos, 
                    pagedResult.TotalItems, 
                    pagedResult.PageNumber, 
                    pagedResult.PageSize
                );

                return Ok(new ApiResponse<PagedResponse<UserDto>>(
                    true, 
                    "Usuarios obtenidos correctamente", 
                    finalPagedData));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en UserController.GetAll");
                return StatusCode(500, new ApiResponse<string>(false, "Error interno: " + ex.Message));
            }
        }

        /// <summary>
        /// Busca un usuario específico por su Email o Nombre de Usuario (Solo Administradores).
        /// </summary>
        /// <param name="search">DTO que contiene el criterio de búsqueda.</param>
        [Authorize(Roles = "Admin")]
        [HttpGet("search")]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetBySearch([FromQuery] UserSearchDto search)
        {
            if (string.IsNullOrEmpty(search.email) && string.IsNullOrEmpty(search.name))
                return BadRequest(new ApiResponse<string>(false, "Se requiere email o nombre"));

            User? user = null;
            if (!string.IsNullOrEmpty(search.name))
                user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserName == search.name);
            else
                user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == search.email);

            if (user == null) return NotFound(new ApiResponse<string>(false, "Usuario no encontrado"));

            return Ok(new ApiResponse<UserDto>(true, "Usuario encontrado", UserMapper.UserToUserDto(user)));
        }

        /// <summary>
        /// Habilita o deshabilita la cuenta de un usuario (Solo Administradores).
        /// </summary>
        /// <remarks>
        /// Por seguridad, el sistema impide la deshabilitación de usuarios con rol de Administrador.
        /// </remarks>
        /// <param name="dto">DTO con el email del usuario a modificar.</param>
        [Authorize(Roles = "Admin")]
        [HttpPatch("status")]
        public async Task<ActionResult<ApiResponse<string>>> ToggleStatus([FromBody] ToggleStatusDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return NotFound(new ApiResponse<string>(false, "Usuario no encontrado"));

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Admin", StringComparer.OrdinalIgnoreCase))
                return BadRequest(new ApiResponse<string>(false, "No se puede deshabilitar a un administrador"));

            user.IsActive = !user.IsActive;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded) return BadRequest(new ApiResponse<string>(false, "Error al actualizar estado"));

            return Ok(new ApiResponse<string>(true, user.IsActive ? "Usuario habilitado" : "Usuario deshabilitado"));
        }

        /// <summary>
        /// Permite al usuario autenticado actualizar sus propios datos de perfil.
        /// </summary>
        /// <param name="dto">DTO con los nuevos datos de perfil.</param>
        [Authorize(Roles = "User")]
        [HttpPut("profile")]
        public async Task<ActionResult<ApiResponse<UserDto>>> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized(new ApiResponse<string>(false, "Sesión inválida"));

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound(new ApiResponse<string>(false, "Usuario no encontrado"));

            UserMapper.UpdateUserFromDto(user, dto);
            var update = await _userManager.UpdateAsync(user);

            if (!update.Succeeded) return BadRequest(new ApiResponse<string>(false, "Error al guardar cambios"));

            return Ok(new ApiResponse<UserDto>(true, "Perfil actualizado", UserMapper.UserToUserDto(user)));
        }

        /// <summary>
        /// Inicia el proceso de recuperación de contraseña enviando un código de 6 dígitos al correo.
        /// </summary>
        /// <remarks>
        /// El código generado tiene una validez de 3 minutos.
        /// </remarks>
        /// <param name="dto">DTO con el email registrado.</param>
        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null) return NotFound("Correo no registrado.");

            string code = new Random().Next(100000, 999999).ToString();

            var resetToken = new PasswordResetToken {
                Email = dto.Email,
                Code = code,
                ExpiryDate = DateTime.UtcNow.AddMinutes(3) 
            };

            _context.PasswordResetTokens.Add(resetToken);
            await _context.SaveChangesAsync();
            await _emailService.SendVerificationCodeAsync(dto.Email, code);

            return Ok("Código enviado con éxito.");
        }

        /// <summary>
        /// Restablece la contraseña utilizando el código de verificación enviado por correo.
        /// </summary>
        /// <param name="dto">DTO con el email, el código de verificación y la nueva contraseña.</param>
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var resetToken = await _context.PasswordResetTokens
                .Where(t => t.Email == dto.Email && t.Code == dto.Code && !t.IsUsed)
                .OrderByDescending(t => t.ExpiryDate)
                .FirstOrDefaultAsync();

            if (resetToken == null || resetToken.ExpiryDate < DateTime.UtcNow)
                return BadRequest("Código inválido o expirado.");

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return NotFound("Usuario no encontrado.");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);

            if (!result.Succeeded) return BadRequest(result.Errors);

            resetToken.IsUsed = true;
            await _context.SaveChangesAsync();

            return Ok("Contraseña actualizada con éxito.");
        }

        /// <summary>
        /// Cambia el rol de un usuario en el sistema (Solo Administradores).
        /// </summary>
        /// <param name="dto">DTO con el email y el nuevo rol solicitado.</param>
        [Authorize(Roles = "Admin")]
        [HttpPatch("changeRole")]
        public async Task<IActionResult> ChangeRole([FromBody] ChangeRoleDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return NotFound(new ApiResponse<string>(false, "Usuario no encontrado"));

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            
            var addResult = await _userManager.AddToRoleAsync(user, dto.NewRole);
            if (!addResult.Succeeded) return BadRequest(new ApiResponse<string>(false, "Error al asignar nuevo rol"));

            user.Role = dto.NewRole;
            await _userManager.UpdateAsync(user);

            return Ok(new ApiResponse<string>(true, $"Rol de {user.Email} actualizado a {dto.NewRole}"));
        }
    }
}