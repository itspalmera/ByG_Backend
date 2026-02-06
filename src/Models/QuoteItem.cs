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
        public int? UnitPrice { get; set; }
        public int? TotalPrice { get; set; }

        //Relacion con Quote
        public int QuoteId { get; set; }
        public Quote Quote { get; set; } = null!;


    }
}