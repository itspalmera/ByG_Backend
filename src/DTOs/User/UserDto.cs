using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.DTOs
{
    public record UserDto
    {

        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public string Registered { get; set; }
        public string? LastAccess { get; set; }
    }
}