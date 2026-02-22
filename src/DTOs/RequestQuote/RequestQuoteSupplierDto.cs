using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.DTOs
{
    public record RequestQuoteSupplierDto
    {
        public DateTime SentAt { get; set; }
        public int RequestQuoteId { get; set; }
        public int SupplierId { get; set; }

    }
}