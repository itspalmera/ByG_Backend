using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ByG_Backend.src.Models
{
    /// <summary>
    /// Representa la Orden de Compra (OC) final generada por el sistema.
    /// Es el documento formal que se envía al proveedor tras aprobar una cotización.
    /// Almacena datos logísticos, financieros y de aprobación para su representación en PDF.
    /// </summary>
    public class PurchaseOrder
    {
        /// <summary>
        /// Identificador único de la orden de compra.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Folio oficial de la orden (ej: OC-2026-001). 
        /// Utilizado para la trazabilidad legal y administrativa.
        /// </summary>
        public string OrderNumber { get; set; } = null!;

        /// <summary>
        /// Fecha de emisión de la orden de compra.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Centro de Costo asignado en ByG para imputar el gasto de esta compra.
        /// </summary>
        public string? CostCenter { get; set; }

        /// <summary>
        /// Comentarios o instrucciones adicionales para el proveedor.
        /// </summary>
        public string? Observations { get; set; }

        /// <summary>
        /// Estado actual de la OC (ej: "Emitida", "Enviada", "Aprobada", "Anulada").
        /// </summary>
        public string Status { get; set; } = "Emitida";
        
        // =========================
        // DATOS DE PAGO Y LOGÍSTICA
        // =========================

        /// <summary>
        /// Medio de pago acordado (ej: Transferencia, Efectivo, Cheque).
        /// </summary>
        public string? PaymentForm { get; set; }

        /// <summary>
        /// Condiciones temporales del pago (ej: "Contado", "30 días").
        /// </summary>
        public string? PaymentTerms { get; set; }

        /// <summary>
        /// Moneda de la transacción (por defecto "CLP").
        /// </summary>
        public string Currency { get; set; } = "CLP";

        /// <summary>
        /// Fecha en la que el proveedor estima entregar los productos.
        /// </summary>
        public DateOnly? ExpectedDeliveryDate { get; set; }

        /// <summary>
        /// Fecha y hora máxima permitida para la recepción de la compra.
        /// </summary>
        public DateTime? DeliveryDeadline { get; set; }

        /// <summary>
        /// Lugar físico donde se deben entregar los bienes.
        /// </summary>
        public string? ShippingAddress { get; set; }

        /// <summary>
        /// Tipo de transporte o logística acordada (ej: "Despacho a Obra", "Retiro en Tienda").
        /// </summary>
        public string? ShippingMethod { get; set; }

        // =========================
        // TOTALES FINANCIEROS (Snapshot)
        // =========================

        /// <summary>
        /// Descuento comercial aplicado al total de la orden.
        /// </summary>
        public decimal Discount { get; set; }

        /// <summary>
        /// Costo adicional por concepto de transporte o flete.
        /// </summary>
        public decimal FreightCharge { get; set; }

        /// <summary>
        /// Suma de los precios unitarios por cantidad antes de impuestos y descuentos.
        /// </summary>
        public decimal SubTotal { get; set; }

        /// <summary>
        /// Monto total que no está afecto a impuestos (IVA).
        /// </summary>
        public decimal TaxExemptTotal { get; set; }

        /// <summary>
        /// Porcentaje de impuesto aplicado (Por defecto 19% para el IVA en Chile).
        /// </summary>
        public decimal TaxRate { get; set; }

        /// <summary>
        /// Monto calculado del impuesto basado en el SubTotal afecto.
        /// </summary>
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// Valor final de la orden de compra (SubTotal - Descuento + Flete + Impuestos).
        /// </summary>
        public decimal TotalAmount { get; set; }

        // =========================
        // FIRMAS Y APROBACIÓN
        // =========================

        /// <summary>
        /// Nombre de la persona que autorizó la emisión de la OC.
        /// </summary>
        public string? ApproverName { get; set; }

        /// <summary>
        /// RUT del aprobador para validación legal.
        /// </summary>
        public string? ApproverRut { get; set; }

        /// <summary>
        /// Cargo o rol del aprobador (ej: "Gerente de Finanzas").
        /// </summary>
        public string? ApproverRole { get; set; }

        /// <summary>
        /// Fecha y hora exacta de la firma digital o aprobación del documento.
        /// </summary>
        public DateTime? SignedAt { get; set; }

        // =========================
        // RELACIONES 
        // =========================
        
        /// <summary>
        /// Identificador de la compra de origen.
        /// </summary>
        public int PurchaseId { get; set; }

        /// <summary>
        /// Propiedad de navegación hacia la solicitud de compra original (Relación 1 a 1).
        /// </summary>
        public Purchase Purchase { get; set; } = null!;

        /// <summary>
        /// Identificador de la cotización que fue seleccionada para generar esta orden.
        /// </summary>
        public int QuoteId { get; set; }

        /// <summary>
        /// Propiedad de navegación hacia la cotización ganadora. 
        /// A través de esta relación se accede al detalle de productos y precios unitarios.
        /// </summary>
        public Quote Quote { get; set; } = null!;
    }
}