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


        // Relaciones

        // QuoteItem N a 1 Quote (padre)
        public List<QuoteItem>? QuoteItems { get; set; }



        // Purchase 1 a N Quote
        public int PurchaseId { get; set; }
        public Purchase Purchase { get; set; } = null!;

        // Supplier 1 a N Quote
        public int SupplierId { get; set; }
        public Supplier Supplier { get; set; } = null!;

        // Quote (principal) 1 a 0..1 PurchaseOrder  (Solo si es aceptada la cotizacion)
        public PurchaseOrder? PurchaseOrder { get; set; }

    }
}