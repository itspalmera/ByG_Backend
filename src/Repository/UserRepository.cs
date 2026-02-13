using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ByG_Backend.src.DTOs;
using ByG_Backend.src.Models;
using Microsoft.AspNetCore.Identity;
using System.Text;
using ByG_Backend.src.Interfaces;
using Resend;
using Microsoft.AspNetCore.WebUtilities;

namespace ByG_Backend.src.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<User> _userManager;
        private readonly IResend _resend;

        public UserRepository(UserManager<User> userManager, IResend resend)
        {
            _userManager = userManager;
            _resend = resend;
        }

        public async Task<bool> SendPasswordResetEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return false;

            // Generar Token usando la infraestructura de Identity (ORM)
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            
            // Codificar para URL
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var callbackUrl = $"https://tu-frontend.com/reset-password?token={encodedToken}&email={email}";

            // Enviar vía Resend
            await _resend.EmailSendAsync(new EmailMessage {
                From = "noreply@tudominio.com",
                To = { email },
                Subject = "Recuperar Contraseña",
                HtmlBody = $"Haga clic <a href='{callbackUrl}'>aquí</a>"
            });

            return true;
        }

        public async Task<IdentityResult> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return IdentityResult.Failed(new IdentityError { Description = "Usuario no encontrado" });

            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(dto.Token));
            
            // El UserManager interactúa con el ORM (Entity Framework) para actualizar la DB
            return await _userManager.ResetPasswordAsync(user, decodedToken, dto.NewPassword);
        }
    }
}
