using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.DTOs
{
    public record UserSearchDto
    {
        public string? email { get; set; }
        public string? name { get; set; }
    }
}