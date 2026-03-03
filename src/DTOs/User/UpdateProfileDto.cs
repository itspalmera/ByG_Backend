using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.DTOs
{
    public record UpdateProfileDto
    {
        public string? FirstName { get; set; } 
        public string? LastName { get; set; } 
        public string? Phone { get; set; }
        public string? Email { get; set; } 
    }
}