using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ByG_Backend.src.DTOs;
using Microsoft.AspNetCore.Identity;
namespace ByG_Backend.src.Interfaces
{
    public interface IUserRepository
    {
         Task<bool> SendPasswordResetEmailAsync(string email);
         Task<IdentityResult> ResetPasswordAsync(ResetPasswordDto dto);
    }
}