using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.DTOs
{
    public record CreateQuoteDto
    {
        [Required]
        public required string Number { get; set; }

        [Required]
        [RegularExpression("^(Pendiente|Aprobada|Rechazada)$", ErrorMessage = "El estado debe ser 'Pendiente', 'Aprobada' o 'Rechazada'.")]
        public required string Status { get; set; } 

        [Required]
        [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "La fecha debe tener el formato DD-MM-AAAA.")]
        public required string Date { get; set; }


        public decimal? TotalPrice { get; set; }

        [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder los 500 caracteres.")]
        public string? Observations { get; set; }

        [Required (ErrorMessage = "La lista de items no puede estar vacía.")]
        public List<CreateQuoteItemDto> QuoteItems { get; set; }


        public int? SupplierId { get; set; } 
        public int? PurchaseId { get; set; }

    }
}