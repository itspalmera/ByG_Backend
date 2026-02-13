using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.DTOs
{
    public class QuoteDto
    {
         public int Number { get; set; }
        public string Status { get; set; }
        public string Date { get; set; }

        public decimal? TotalPrice { get; set; }
        public required string[] Items { get; set; }
    }
}