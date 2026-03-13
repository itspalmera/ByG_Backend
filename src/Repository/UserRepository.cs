using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ByG_Backend.src.Data;
using ByG_Backend.src.DTOs;
using ByG_Backend.src.Interfaces;
using ByG_Backend.src.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Repositorio encargado de la gestión de persistencia de usuarios y lógica de seguridad extendida.
/// Implementa <see cref="IUserRepository"/> integrando tanto el UserManager de Identity como el contexto de datos personalizado.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly UserManager<User> _userManager;
    private readonly DataContext _context;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="UserRepository"/>.
    /// </summary>
    /// <param name="userManager">Servicio de Identity para la gestión de usuarios.</param>
    /// <param name="context">Contexto de base de datos para acceder a tablas personalizadas como PasswordResetTokens.</param>
    public UserRepository(UserManager<User> userManager, DataContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    /// <summary>
    /// Realiza el restablecimiento de la contraseña validando un código de verificación manual.
    /// </summary>
    /// <remarks>
    /// El proceso sigue este flujo:
    /// 1. Busca y valida el código manual (OTP) en la tabla PasswordResetTokens.
    /// 2. Verifica la vigencia del código (ExpiryDate).
    /// 3. Genera un token interno de Identity de forma dinámica para autorizar el cambio.
    /// 4. Ejecuta el ResetPassword a través del UserManager.
    /// 5. Invalida el código manual marcándolo como usado tras el éxito.
    /// </remarks>
    /// <param name="dto">DTO que contiene el Email, el código de verificación y la nueva contraseña.</param>
    /// <returns>Un <see cref="IdentityResult"/> indicando si la operación fue exitosa o los errores encontrados.</returns>
    public async Task<IdentityResult> ResetPasswordAsync(ResetPasswordDto dto)
    {
        var resetToken = await _context.PasswordResetTokens
            .Where(t => t.Email == dto.Email && t.Code == dto.Code && !t.IsUsed)
            .OrderByDescending(t => t.ExpiryDate)
            .FirstOrDefaultAsync();

        if (resetToken == null)
            return IdentityResult.Failed(new IdentityError { Description = "Código de verificación inválido." });

        if (resetToken.ExpiryDate < DateTime.UtcNow)
            return IdentityResult.Failed(new IdentityError { Description = "El código ha expirado." });

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null) 
            return IdentityResult.Failed(new IdentityError { Description = "Usuario no encontrado." });

        var internalToken = await _userManager.GeneratePasswordResetTokenAsync(user);

        var result = await _userManager.ResetPasswordAsync(user, internalToken, dto.NewPassword);

        if (result.Succeeded)
        {
            resetToken.IsUsed = true;
            _context.PasswordResetTokens.Update(resetToken);
            await _context.SaveChangesAsync();
        }

        return result;
    }
}