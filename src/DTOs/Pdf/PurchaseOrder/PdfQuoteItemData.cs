using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.DTOs
{
    public class PdfQuoteItemData
    {
        public string Description { get; set; } = null!; // descripción del producto
    public int Quantity { get; set; }            // cantidad
    public string Unit { get; set; } = null!;        // unidad (kg, unidad, metro, etc)

    public string? Brand { get; set; }               // marca (opcional)
    public string? Model { get; set; }               // modelo (opcional)

    public decimal UnitPrice { get; set; }           // precio unitario
    public decimal Total { get; set; }  
    }
}