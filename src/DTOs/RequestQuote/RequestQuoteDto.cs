using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ByG_Backend.src.Models;
using ByG_Backend.src.DTOs;

namespace ByG_Backend.src.DTOs
{
    public record RequestQuoteDto
    {
        public int Id { get; set; }
        public string Number { get; set; } = null!;

        public string Status { get; set; }

        public string CreatedAt { get; set; }
        public string? SentAt { get; set; }


        public int PurchaseId { get; set; }
        public string PurchaseDescription { get; set; } = string.Empty;

        public List<RequestQuoteSupplierDto> RequestQuoteSuppliers { get; set; } = new();
    }

}