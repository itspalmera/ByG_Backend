public class PdfPurchaseOrderData
{
    public string OrderNumber { get; set; } = null!;
    public DateTime Date { get; set; }

    public string? CostCenter { get; set; }

    public string? PaymentForm { get; set; }
    public string? PaymentTerms { get; set; }

    public string Currency { get; set; } = "CLP";

    public decimal Discount { get; set; }
    public decimal FreightCharge { get; set; }

    public decimal SubTotal { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
}