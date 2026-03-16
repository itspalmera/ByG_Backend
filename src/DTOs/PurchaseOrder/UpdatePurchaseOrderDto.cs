using System;

namespace ByG_Backend.src.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos para la actualización flexible de Órdenes de Compra (OC).
    /// Permite modificar términos comerciales, logísticos y financieros una vez emitida la orden.
    /// </summary>
    public record UpdatePurchaseOrderDto
    {
        /// <summary>
        /// Centro de costo asignado para la imputación contable del gasto.
        /// </summary>
        public string? CostCenter { get; set; }

        /// <summary>
        /// Forma de pago acordada (ej: Transferencia, Cheque, Efectivo).
        /// </summary>
        public string? PaymentForm { get; set; }

        /// <summary>
        /// Condiciones o plazos de pago (ej: "30 días contra factura").
        /// </summary>
        public string? PaymentTerms { get; set; }

        /// <summary>
        /// Dirección física donde el proveedor debe realizar la entrega de los materiales.
        /// </summary>
        public string? ShippingAddress { get; set; }

        /// <summary>
        /// Método o empresa de transporte encargada del despacho.
        /// </summary>
        public string? ShippingMethod { get; set; }

        /// <summary>
        /// Notas adicionales para el proveedor o instrucciones especiales de entrega.
        /// </summary>
        public string? Observations { get; set; }

        /// <summary>
        /// Fecha estimada de llegada de los productos a obra o bodega.
        /// </summary>
        public DateOnly? ExpectedDeliveryDate { get; set; }

        /// <summary>
        /// Fecha y hora límite fatal para la recepción del pedido.
        /// </summary>
        public DateTime? DeliveryDeadline { get; set; }

        /// <summary>
        /// Monto de descuento aplicado al total de la orden.
        /// </summary>
        public decimal? Discount { get; set; }

        /// <summary>
        /// Cargo adicional por concepto de transporte o logística.
        /// </summary>
        public decimal? FreightCharge { get; set; }
    }
}