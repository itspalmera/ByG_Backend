using System.Collections.Generic;

namespace ByG_Backend.src.DTOs
{
    public record PurchaseOrderDetailDto
    {
        // --- Encabezado ---
        public int Id { get; set; }
        public string OrderNumber { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string Date { get; set; } = string.Empty;
        public string? CostCenter { get; set; } // Viene de Purchase o se ingresa manual

        // --- Referencias ---
        public int PurchaseId { get; set; }
        public string PurchaseNumber { get; set; } = null!; // Folio Solicitud
        public string ProjectName { get; set; } = null!;

        // --- Datos del Proveedor (Flattened from Quote.Supplier) ---
        public SupplierInfoDto Supplier { get; set; } = null!;

        // --- Logística y Pago ---
        public string? PaymentForm { get; set; }
        public string? PaymentTerms { get; set; }
        public string Currency { get; set; } = "CLP";
        public DateOnly? ExpectedDeliveryDate { get; set; }
        public string? DeliveryDeadline { get; set; }
        public string? ShippingAddress { get; set; }
        public string? ShippingMethod { get; set; }
        public string? Observations { get; set; }

        // --- Items (Flattened from Quote.QuoteItems) ---
        public List<PurchaseOrderItemDto> Items { get; set; } = new();

        // --- Totales (Snapshot guardado en BD) ---
        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal FreightCharge { get; set; }
        public decimal TaxExemptTotal { get; set; } // Neto exento
        public decimal TaxRate { get; set; } // 19%
        public decimal TaxAmount { get; set; } // IVA
        public decimal TotalAmount { get; set; }

        // --- Aprobación ---
        public string? ApproverName { get; set; }
        public string? ApproverRole { get; set; }
        public string? SignedAt { get; set; }
    }

    // Sub-DTO para info del proveedor dentro de la OC
    public record SupplierInfoDto(
        string Rut,
        string BusinessName,
        string Email,
        string? Phone,
        string? Address,
        string? City,
        string? ContactName
    );

    // Sub-DTO para los items (Mapeado desde QuoteItem)
    public record PurchaseOrderItemDto(
        string Name,
        string? Description,
        string Unit,
        int Quantity,
        decimal UnitPrice,
        decimal TotalPrice
    );
}