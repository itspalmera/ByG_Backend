using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace ByG_Backend.src.Models
{
    /// <summary>
    /// Usuario del sistema
    /// </summary>
    public class User : IdentityUser
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Role { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public DateOnly Registered { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
        public DateTime? LastAccess { get; set; }
    }
}