using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ByG_Backend.src.DTOs;
using Microsoft.AspNetCore.Identity;

namespace ByG_Backend.src.Interfaces
{
    public interface IEmailService
    {
        Task<IdentityResult> ResetPasswordAsync(ResetPasswordDto dto);
        Task SendVerificationCodeAsync(string email, string code);
        Task SendPdfDocumentAsync(string email, byte[] pdfBytes, string fileName);
    }
}