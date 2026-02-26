namespace ByG_Backend.src.DTOs
{
    public record UpdatePurchaseOrderDto
    {
        public string? CostCenter { get; set; }
        public string? PaymentForm { get; set; }
        public string? PaymentTerms { get; set; }
        public string? ShippingAddress { get; set; }
        public string? ShippingMethod { get; set; }
        public string? Observations { get; set; }
        public DateOnly? ExpectedDeliveryDate { get; set; }
        public DateTime? DeliveryDeadline { get; set; }
        public decimal? Discount { get; set; }
        public decimal? FreightCharge { get; set; }
        
        // El estado se suele manejar en un endpoint separado (PATCH status), 
        // pero se puede incluir aquí si es edición simple.
    }
}