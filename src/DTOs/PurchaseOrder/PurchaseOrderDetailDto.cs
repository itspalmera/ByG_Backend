using System.Collections.Generic;

namespace ByG_Backend.src.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos exhaustivo para la visualización de una Orden de Compra (OC).
    /// Consolida información administrativa, logística, financiera y de aprobación en una sola estructura.
    /// </summary>
    public record PurchaseOrderDetailDto
    {
        // --- Encabezado ---
        public int Id { get; set; }
        public string OrderNumber { get; set; } = null!; // Folio OC (ej: OC-2026-0001)
        public string Status { get; set; } = null!;
        public string Date { get; set; } = string.Empty;
        public string? CostCenter { get; set; } // Centro de costo para imputación contable

        // --- Referencias ---
        public int PurchaseId { get; set; }
        public string PurchaseNumber { get; set; } = null!; // Vínculo con la solicitud origen
        public string ProjectName { get; set; } = null!;

        // --- Datos del Proveedor ---
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

        // --- Items ---
        public List<PurchaseOrderItemDto> Items { get; set; } = new();

        // --- Totales (Cálculos tributarios) ---
        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal FreightCharge { get; set; }
        public decimal TaxExemptTotal { get; set; } // Monto neto exento de impuestos
        public decimal TaxRate { get; set; } // Tasa aplicada (ej: 0.19)
        public decimal TaxAmount { get; set; } // Valor del IVA
        public decimal TotalAmount { get; set; }

        // --- Trazabilidad y Aprobación ---
        public string? ApproverName { get; set; }
        public string? ApproverRole { get; set; }
        public string? SignedAt { get; set; }
    }

    /// <summary>
    /// Información de contacto y legal del proveedor específica para el documento de compra.
    /// </summary>
    public record SupplierInfoDto(
        string Rut,
        string BusinessName,
        string Email,
        string? Phone,
        string? Address,
        string? City,
        string? ContactName
    );

    /// <summary>
    /// Detalle técnico y económico de cada ítem adjudicado en la orden.
    /// </summary>
    public record PurchaseOrderItemDto(
        string Name,
        string? Description,
        string Unit,
        int Quantity,
        decimal UnitPrice,
        decimal TotalPrice
    );
}