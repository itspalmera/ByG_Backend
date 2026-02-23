using System.ComponentModel.DataAnnotations;

namespace ByG_Backend.src.DTOs
{
    public record CreatePurchaseOrderDto
    {
        [Required]
        public int PurchaseId { get; set; }

        [Required]
        public int QuoteId { get; set; } // La cotización ganadora

        // Datos logísticos que el usuario define al momento de generar la OC
        public string? PaymentForm { get; set; } // Ej: Transferencia
        public string? PaymentTerms { get; set; } // Ej: 30 días
        
        public DateOnly? ExpectedDeliveryDate { get; set; }
        
        public string? ShippingAddress { get; set; } 
        public string? ShippingMethod { get; set; }
        
        public string? Observations { get; set; }

        // Datos del aprobador (pueden venir del token del usuario logueado, 
        // pero si se maneja explícito en el front, se reciben aquí)
        public string? ApproverName { get; set; }
        public string? ApproverRut { get; set; }
        public string? ApproverRole { get; set; }
    }
}