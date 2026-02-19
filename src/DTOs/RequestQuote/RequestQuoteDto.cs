using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ByG_Backend.src.Models;
using ByG_Backend.src.DTOs;

namespace ByG_Backend.src.DTOs
{
    public class RequestQuoteDto
    {
        public string Number { get; set; } = null!;

        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? SentAt { get; set; }


        public int PurchaseId { get; set; }
        public string PurchaseDescription { get; set; } = string.Empty;

        public List<RequestQuoteSupplierDto> RequestQuoteSuppliers { get; set; } = new();
    }

}