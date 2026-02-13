using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore; // For attributes like [Required], [MaxLength]
using System.ComponentModel.DataAnnotations;

namespace ByG_Backend.src.Models
{
    public class Supplier
    {
        public int Id { get; set; }

        public string Rut { get; set; } = null!; // Id unico chile
        public string BusinessName { get; set; } = null!; // Razón Social
        public string? ContactName { get; set; } // Nombre persona contacto
        public string Email { get; set; } = null!;
        public string? Phone { get; set; } 
        public string? Address { get; set; } 
        public string? City { get; set; }

        
        // Categorías de productos que maneja (ej: "EPP, Construcción")
        public string? ProductCategories { get; set; } 
        
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;


        // Relaciones

        // requestQuoteSupplier N a 1 supplier
        public List<RequestQuoteSupplier> RequestQuoteSuppliers { get; set; } = new();
        
        // quote N a 1 supplier
        public List<Quote> Quotes { get; set; } = new();
    
    }
}
             