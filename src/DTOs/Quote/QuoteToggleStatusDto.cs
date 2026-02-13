using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.DTOs
{
    public class QuoteToggleStatusDto
    {

        public int id { get; set; }

        [Required]
        public string newStatus { get; set; } = string.Empty;
    }
}