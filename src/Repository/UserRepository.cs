using ByG_Backend.src.Data;
using ByG_Backend.src.DTOs;
using ByG_Backend.src.Interfaces;
using ByG_Backend.src.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class UserRepository : IUserRepository
{
    private readonly UserManager<User> _userManager;
    private readonly DataContext _context; // Necesario para ver tus códigos manuales

    public UserRepository(UserManager<User> userManager, DataContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<IdentityResult> ResetPasswordAsync(ResetPasswordDto dto)
    {
        // 1. Validar el código manual en tu tabla de ByG
        var resetToken = await _context.PasswordResetTokens
            .Where(t => t.Email == dto.Email && t.Code == dto.Code && !t.IsUsed)
            .OrderByDescending(t => t.ExpiryDate)
            .FirstOrDefaultAsync();

        if (resetToken == null)
            return IdentityResult.Failed(new IdentityError { Description = "Código de verificación inválido." });

        if (resetToken.ExpiryDate < DateTime.UtcNow)
            return IdentityResult.Failed(new IdentityError { Description = "El código ha expirado." });

        // 2. Buscar al usuario
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null) 
            return IdentityResult.Failed(new IdentityError { Description = "Usuario no encontrado." });

        // 3. Generar el Token interno de Identity "al vuelo"
        // Esto soluciona el error CS1061 porque ya no usamos dto.Token
        var internalToken = await _userManager.GeneratePasswordResetTokenAsync(user);

        // 4. Cambiar la contraseña
        var result = await _userManager.ResetPasswordAsync(user, internalToken, dto.NewPassword);

        if (result.Succeeded)
        {
            // 5. Marcar código como usado
            resetToken.IsUsed = true;
            _context.PasswordResetTokens.Update(resetToken);
            await _context.SaveChangesAsync();
        }

        return result;
    }

}