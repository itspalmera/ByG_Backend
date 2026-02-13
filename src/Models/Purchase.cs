using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ByG_Backend.src.Models
{
    public class Purchase
    {
        public int Id { get; set; }
        public string PurchaseNumber { get; set; } = null!; // Folio interno 
        public string ProjectName { get; set; } = null!; // Nombre de la obra/proyecto
        public string Status { get; set; } = null!;
        public DateTime RequestDate { get; set; } 
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow; // al editar/agregar cotizaciones, generar OC, etc
        public string Requester { get; set; } = null!; // Quién pide desde terreno 
        public string? Observations { get; set; }


        // Relaciones

        // purchaseItems N a 1 purchase
        public List<PurchaseItem> PurchaseItems { get; set; } = new(); 
        // quotes N a 1 purchase
        public List<Quote> Quotes { get; set; } = new(); // N Quotes por compra
        // requestquote 1 a 1 purchase (principal)
        public RequestQuote? RequestQuote { get; set; } // 1 a 1
        // purchaseorder 1 a 1 purchase (principal)
        public PurchaseOrder? PurchaseOrder { get; set; } // 1 a 1 final
    }
}
      