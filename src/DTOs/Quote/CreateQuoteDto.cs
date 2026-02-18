using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.DTOs
{
    public class CreateQuoteDto
    {
        public string Number { get; set; }
        public string Status { get; set; }
        public string Date { get; set; }

        public decimal? TotalPrice { get; set; }

        public List<CreateQuoteItemDto> QuoteItems { get; set; }


        public int? SupplierId { get; set; } 
        public int? PurchaseId { get; set; }

    }
}