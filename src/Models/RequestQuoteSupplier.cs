using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.Models
{
    public class RequestQuoteSupplier
    {
        public int Id { get; set; }

        // Cuándo se envió la solicitud a este proveedor
        public DateTime SentAt { get; set; }

        // Relaciones

        // Supplier 1 a N RequestQuoteSupplier
        public int SupplierId { get; set; }
        public Supplier Supplier { get; set; } = null!;

        // RequestQuote 1 a N RequestQuoteSupplier
        public int RequestQuoteId { get; set; }
        public RequestQuote RequestQuote { get; set; } = null!;

        
    }
}