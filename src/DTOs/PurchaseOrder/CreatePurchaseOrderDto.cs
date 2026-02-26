using System.ComponentModel.DataAnnotations;

namespace ByG_Backend.src.DTOs
{
    public record CreatePurchaseOrderDto
    {
        [Required]
        public int PurchaseId { get; set; }

        [Required]
        public int QuoteId { get; set; } // La cotización ganadora


        // --- Datos de Formalización (Logística y Financiera) ---

        public string? CostCenter { get; set; } // Centro de costo asociado a la compra

        // Datos logísticos que el usuario define al momento de generar la OC
        public string? PaymentForm { get; set; } // Ej: Transferencia
        public string? PaymentTerms { get; set; } // Ej: 30 días
        
        public DateOnly? ExpectedDeliveryDate { get; set; }
        public DateTime? DeliveryDeadline { get; set; } // Fecha límite de entrega (puede ser igual a ExpectedDeliveryDate o más estricta)
        
        public string? ShippingAddress { get; set; } 
        public string? ShippingMethod { get; set; }
        
        public string? Observations { get; set; }

        public string Currency { get; set; } = "CLP"; // Moneda de la compra, por defecto CLP

        // Totales adicionales que se definen al cierre
        public decimal Discount { get; set; } = 0; // Descuento total aplicado a la compra
        public decimal FreightCharge { get; set; } = 0; // Costo de flete


        // Datos del Firmante aprobador (pueden venir del token del usuario logueado, 
        // pero si se maneja explícito en el front, se reciben aquí)
        public string? ApproverName { get; set; }
        public string? ApproverRut { get; set; }
        public string? ApproverRole { get; set; }
    }
}