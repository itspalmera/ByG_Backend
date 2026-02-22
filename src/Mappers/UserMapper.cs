using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ByG_Backend.src.DTOs;
using ByG_Backend.src.Models;

namespace ByG_Backend.src.Mappers
{
    public class UserMapper
    {
         public static User RegisterToUser(RegisterDto dto) =>
            new()
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Role = dto.Role,
                Registered = DateOnly.FromDateTime(DateTime.UtcNow),
                IsActive = true,
            };




        public static UserDto UserToUserDto(User user) =>
        new()
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            IsActive = user.IsActive,
            Role = user.Role,
            Registered = user.Registered.ToString("dd/MM/yyyy"), 
            LastAccess = user.LastAccess.HasValue 
            ? user.LastAccess.Value.ToString("dd/MM/yyyy 'T' HH:mm:ss") 
            : "Sin acceso"
        };



        public static NewUserDto UserToNewUserDto(User user) =>
        new()
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email
        };



        public static AuthenticatedUserDto UserToAuthenticatedDto(User user, string token, string role) =>
        new()
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            Phone = user.PhoneNumber ?? string.Empty,
            Token = token,
            Role = role,
            IsActive = user.IsActive,
            Registered = user.Registered,
            LastAccess = user.LastAccess
        };



        public static void UpdateUserFromDto(User user, UpdateProfileDto dto)
        {
            if (!string.IsNullOrWhiteSpace(dto.FirstName))
                user.FirstName = dto.FirstName.Trim();

            if (!string.IsNullOrWhiteSpace(dto.LastName))
                user.LastName = dto.LastName.Trim();

            if (!string.IsNullOrWhiteSpace(dto.Email))
                user.Email = dto.Email.Trim().ToLower();

            if (!string.IsNullOrWhiteSpace(dto.Phone))
                user.PhoneNumber = dto.Phone.Trim();
        }

    }
}