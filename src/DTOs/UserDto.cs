using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.DTOs
{
    public class UserDto
    {

        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public DateOnly Registered { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
        public DateOnly? LastAccess { get; set; }
    }
}