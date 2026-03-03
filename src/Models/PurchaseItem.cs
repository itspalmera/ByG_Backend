using System;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ByG_Backend.src.Models
{
    public class PurchaseItem
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!; // Material/Product Name
        public string? BrandModel {get; set;} //Marca/Modelo
        public string? Description { get; set; }
        public string Unit { get; set; } = null!; // Unit of Measurement (e.g., UN, Saco, KG, MT) Ej. Unidad
        public string? Size { get; set; } // Talla/Medida Ej. XL
        public int Quantity { get; set; }


        // Relaciones

        // relacion PurchaseItem N a 1 Purchase
        public int PurchaseId { get; set; }
        public Purchase Purchase { get; set; } = null!;

        // relacion QuoteItem N a 1 PurchaseItem
        // relación con las cotizaciones recibidas para este ítem específico, es decir, precios posibles para cada producto
        public List<QuoteItem> QuoteItems { get; set; } = new();
    
    }
}