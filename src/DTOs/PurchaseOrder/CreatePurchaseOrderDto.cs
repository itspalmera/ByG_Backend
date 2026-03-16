using System.ComponentModel.DataAnnotations;

namespace ByG_Backend.src.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos para la generación de una nueva Orden de Compra (OC).
    /// Consolida la decisión de adjudicación y formaliza los términos logísticos y financieros.
    /// </summary>
    public record CreatePurchaseOrderDto
    {
        /// <summary>
        /// ID de la solicitud de compra original que dio origen al proceso.
        /// </summary>
        [Required(ErrorMessage = "El ID de la compra es obligatorio.")]
        public int PurchaseId { get; set; }

        /// <summary>
        /// ID de la cotización seleccionada como ganadora para esta orden.
        /// </summary>
        [Required(ErrorMessage = "Debe seleccionar una cotización válida.")]
        public int QuoteId { get; set; }

        // --- Datos de Formalización (Logística y Financiera) ---

        /// <summary>
        /// Centro de costo para la imputación contable de la obra o departamento.
        /// </summary>
        public string? CostCenter { get; set; }

        /// <summary>
        /// Método de pago acordado con el proveedor (ej: Transferencia electrónica).
        /// </summary>
        public string? PaymentForm { get; set; }

        /// <summary>
        /// Plazos de pago convenidos (ej: "Neto 30 días", "Contra entrega").
        /// </summary>
        public string? PaymentTerms { get; set; }
        
        /// <summary>
        /// Fecha estimada en que se espera recibir el suministro en terreno.
        /// </summary>
        public DateOnly? ExpectedDeliveryDate { get; set; }

        /// <summary>
        /// Fecha y hora máxima fatal para la recepción de los materiales.
        /// </summary>
        public DateTime? DeliveryDeadline { get; set; }
        
        /// <summary>
        /// Punto de entrega físico de los materiales o servicios.
        /// </summary>
        public string? ShippingAddress { get; set; } 

        /// <summary>
        /// Modo de transporte o courier encargado del despacho.
        /// </summary>
        public string? ShippingMethod { get; set; }
        
        /// <summary>
        /// Instrucciones especiales o notas de aclaración para el proveedor.
        /// </summary>
        public string? Observations { get; set; }

        /// <summary>
        /// Divisa en la que se transa la operación. Por defecto: CLP.
        /// </summary>
        public string Currency { get; set; } = "CLP";

        /// <summary>
        /// Monto de descuento global aplicado sobre el neto de la cotización.
        /// </summary>
        public decimal Discount { get; set; } = 0;

        /// <summary>
        /// Cargos adicionales por concepto de transporte o seguros de carga.
        /// </summary>
        public decimal FreightCharge { get; set; } = 0;

        // --- Datos del Firmante (Trazabilidad de Aprobación) ---

        /// <summary>
        /// Nombre del usuario con rol de "Autorizador" que emite la orden.
        /// </summary>
        public string? ApproverName { get; set; }

        /// <summary>
        /// RUT del autorizador para firma digital o validación interna.
        /// </summary>
        public string? ApproverRut { get; set; }

        /// <summary>
        /// Cargo o rol jerárquico del firmante al momento de la aprobación.
        /// </summary>
        public string? ApproverRole { get; set; }
    }
}