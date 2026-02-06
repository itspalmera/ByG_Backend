using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.Models
{
    /// <summary>
    /// Cotización que llega de un proveedor
    /// </summary>
    public class Quote
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public string Status { get; set; }
        public DateOnly Date { get; set; }

        public decimal? TotalPrice { get; set; }
        public List<QuoteItem>? Items { get; set; }

    }
}