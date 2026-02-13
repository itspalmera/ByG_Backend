using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using ByG_Backend.src.Data;
using ByG_Backend.src.DTOs;
using ByG_Backend.src.Helpers;
using ByG_Backend.src.Mappers;
using ByG_Backend.src.Models;
using ByG_Backend.src.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Resend;
using ByG_Backend.src.DTO;

namespace ByG_Backend.src.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController(ILogger<UserController> logger, DataContext context, UserManager<User> userManager, IResend resend, IUserRepository repository) : ControllerBase
    {
        private readonly ILogger<UserController> _logger = logger;
        private readonly DataContext _context = context;
        private readonly UserManager<User> _userManager = userManager;
        private readonly IResend _resend = resend;
        private readonly IUserRepository _repository = repository;

        // =========================
        // GET ALL (Admin) + filtros inline (sin UserParams, sin metadata)
        // =========================
        //[Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<UserDto>>>> GetAll(
            [FromQuery] bool? isActive,
            [FromQuery] string? searchTerm,
            [FromQuery] DateOnly? registeredFrom,
            [FromQuery] DateOnly? registeredTo,
            [FromQuery] string? orderBy,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10
        )
        {
            // sanitizar paginación
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var query = _context.Users.AsNoTracking().AsQueryable();

            // 1) Filtro por activo
            if (isActive.HasValue)
            {
                query = query.Where(u => u.IsActive == isActive.Value);
            }


            if (registeredFrom.HasValue)
            {
                query = query.Where(u => u.Registered >= registeredFrom.Value);
            }

            if (registeredTo.HasValue)
            {
                // opcional: incluir todo el día si te pasan solo fecha
                var to = registeredTo.Value;
                query = query.Where(u => u.Registered <= to);
            }

            // 3) Search (email / username)
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();

                query = query.Where(u =>
                    (u.Email != null && u.Email.ToLower().Contains(term)) ||
                    (u.UserName != null && u.UserName.ToLower().Contains(term))
                );
            }

            // 4) Ordenamiento simple
            //    orderBy ejemplos:
            //    - "email"
            //    - "email:desc"
            //    - "username"
            //    - "createdAt:desc"
            //    - "lastAccess"
            var key = (orderBy ?? "createdAt:desc").Trim().ToLower();
            var desc = key.EndsWith(":desc");
            if (desc) key = key.Replace(":desc", "");

            query = key switch
            {
                "email" => desc ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
                "username" => desc ? query.OrderByDescending(u => u.UserName) : query.OrderBy(u => u.UserName),
                "lastaccess" => desc ? query.OrderByDescending(u => u.LastAccess) : query.OrderBy(u => u.LastAccess),
                "createdat" => desc ? query.OrderByDescending(u => u.Registered) : query.OrderBy(u => u.Registered),
                _ => query.OrderByDescending(u => u.Registered)
            };

            // (opcional) total para que el frontend sepa
            var total = await query.CountAsync();

            // 5) Paginación
            var users = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = users.Select(UserMapper.UserToUserDto).ToList();

            return Ok(new ApiResponse<IEnumerable<UserDto>>(
                true,
                $"Usuarios obtenidos correctamente. Total: {total}",
                dtos
            ));
        }

        // =========================
        // SEARCH (Admin) by email OR name
        // =========================
        //[Authorize(Roles = "Admin")]
        [HttpGet("search")]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetById([FromQuery] UserSearchDto search)
        {
            if (string.IsNullOrEmpty(search.email) && string.IsNullOrEmpty(search.name))
            {
                return BadRequest(new ApiResponse<string>(false, "Se requiere un email o nombre para buscar el usuario"));
            }

            if (!string.IsNullOrEmpty(search.name))
            {
                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UserName == search.name);

                if (user == null)
                    return NotFound(new ApiResponse<string>(false, "Usuario no encontrado"));

                var dto = UserMapper.UserToUserDto(user);
                return Ok(new ApiResponse<UserDto>(true, "Usuario encontrado", dto));
            }
            else
            {
                var userByEmail = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == search.email);

                if (userByEmail == null)
                    return NotFound(new ApiResponse<string>(false, "Usuario no encontrado (email)"));

                var dtoByEmail = UserMapper.UserToUserDto(userByEmail);
                return Ok(new ApiResponse<UserDto>(true, "Usuario encontrado", dtoByEmail));
            }
        }

        // =========================
        // TOGGLE STATUS (Admin)
        // =========================
        //[Authorize(Roles = "Admin")]
        [HttpPatch("status")]
        public async Task<ActionResult<ApiResponse<string>>> ToggleStatus([FromBody] ToggleStatusDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return NotFound(new ApiResponse<string>(false, "Usuario no encontrado"));

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Admin", StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest(new ApiResponse<string>(
                    false,
                    "No se puede deshabilitar una cuenta con rol de administrador."
                ));
            }

            user.IsActive = !user.IsActive;

            var update = await _userManager.UpdateAsync(user);
            if (!update.Succeeded)
            {
                return StatusCode(500, new ApiResponse<string>(
                    false,
                    "Error al actualizar el estado del usuario",
                    null,
                    update.Errors.Select(e => e.Description).ToList()
                ));
            }

            var message = user.IsActive ? "Usuario habilitado correctamente" : "Usuario deshabilitado correctamente";
            return Ok(new ApiResponse<string>(true, message));
        }

        // =========================
        // UPDATE PROFILE (User)
        // =========================
        //[Authorize(Roles = "User")]
        [HttpPut("profile")]
        public async Task<ActionResult<ApiResponse<UserDto>>> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
                return Unauthorized(new ApiResponse<string>(false, "Usuario no autenticado"));

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return NotFound(new ApiResponse<string>(false, "Usuario no encontrado"));

            UserMapper.UpdateUserFromDto(user, dto);

            var update = await _userManager.UpdateAsync(user);
            if (!update.Succeeded)
            {
                return StatusCode(500, new ApiResponse<string>(
                    false,
                    "Error al actualizar el perfil",
                    null,
                    update.Errors.Select(e => e.Description).ToList()
                ));
            }

            return Ok(new ApiResponse<UserDto>(true, "Perfil actualizado correctamente", UserMapper.UserToUserDto(user)));
        }

        /**
        // =========================
        // CHANGE PASSWORD (User)
        // =========================
        //[Authorize(Roles = "User")]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            // El framework valida automáticamente el Email antes de entrar aquí
            var result = await _repository.SendPasswordResetEmailAsync(dto.Email);
            
            // Siempre devolvemos Ok por seguridad (evita enumeración de usuarios)
            return Ok(new ApiResponse<string>(true, "Si el correo existe, se ha enviado un enlace de recuperación."));
        }

        //[Authorize(Roles = "User")]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var result = await _repository.ResetPasswordAsync(dto);
            
            if (!result.Succeeded)
                return BadRequest(new ApiResponse<string>(false, "Error al restablecer", null, 
                    result.Errors.Select(e => e.Description).ToList()));

            return Ok(new ApiResponse<string>(true, "Contraseña actualizada con éxito."));
        
        }
        **/


        //[Authorize(Roles = "Admin")]
        [HttpPatch("changeRole")]
        public async Task<IActionResult> ChangeRole([FromBody] ChangeRoleDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ApiResponse<string>(false, "Datos inválidos"));

                
                var user = await _userManager.FindByEmailAsync(dto.Email);
                if (user == null)
                    return NotFound(new ApiResponse<string>(false, "Usuario no encontrado"));

                
                var currentRoles = await _userManager.GetRolesAsync(user);
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                
                if (!removeResult.Succeeded)
                    return StatusCode(500, new ApiResponse<string>(false, "Error al limpiar roles antiguos"));

                
                var addResult = await _userManager.AddToRoleAsync(user, dto.NewRole);
                if (!addResult.Succeeded)
                {
                    
                    await _userManager.AddToRolesAsync(user, currentRoles);
                    return BadRequest(new ApiResponse<string>(false, $"El rol '{dto.NewRole}' no es válido o no existe."));
                }

                
                user.Role = dto.NewRole;
                var updateResult = await _userManager.UpdateAsync(user);

                if (!updateResult.Succeeded)
                    return StatusCode(500, new ApiResponse<string>(false, "Rol cambiado en Identity pero falló la actualización del perfil."));

                return Ok(new ApiResponse<string>(true, $"Rol de {user.Email} actualizado a {dto.NewRole} correctamente."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar rol del usuario {Email}", dto.Email);
                return StatusCode(500, new ApiResponse<string>(false, "Error interno del servidor", null, new List<string> { ex.Message }));
            }
        }
    }
}
