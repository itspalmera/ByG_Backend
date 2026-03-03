using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.Models
{
    /// <summary>
    /// Item de una solicitud de cotización 
    /// </summary>
    public class QuoteItem
    {
        public int Id { get; set; }
        public int LineNumber { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string Unit { get; set; }
        public int Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? TotalPrice { get; set; }

        // Relaciones

        // Quote 1 a N QuoteItem
        public int QuoteId { get; set; }
        public Quote Quote { get; set; } = null!;

        // PurchaseItem 1 a N QuoteItem (PurchaseItemId) para saber que produto de la solicitud se cotiza
        public int? PurchaseItemId { get; set; }
        public PurchaseItem? PurchaseItem { get; set; } = null!;

    }
}