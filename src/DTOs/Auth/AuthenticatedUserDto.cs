using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.DTOs
{
    public record AuthenticatedUserDto
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;

        public string Token { get; set; } = null!;
        public string Role { get; set; } = null!;


        public DateOnly Registered { get; set; }
        public DateTime? LastAccess { get; set; }
        public bool IsActive { get; set; }
    }
}