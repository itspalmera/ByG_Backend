using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.Models
{
    public class RequestQuoteSupplier
    {
        public int Id { get; set; }

        // FK a la solicitud de cotización
        public int RequestQuoteId { get; set; }
        public RequestQuote RequestQuote { get; set; } = null!;

        // Cuándo se envió la solicitud a este proveedor
        public DateTime SentAt { get; set; }
    }
}