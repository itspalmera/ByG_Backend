using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.DTOs
{
    public class NewUserDto
    {
        public string FirstName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;
    }
}