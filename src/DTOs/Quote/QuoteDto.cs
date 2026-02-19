using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ByG_Backend.src.Models;

namespace ByG_Backend.src.DTOs
{
    public record QuoteDto
    {
        public string id { get; set; }
        public string Number { get; set; }
        public string Status { get; set; }
        public string Date { get; set; }

        public decimal? TotalPrice { get; set; }

        public string? Observations { get; set; }
        public String? SupplierName { get; set; }
        public List<QuoteItemDetailDto> Items { get; set; }

        
    }

}