using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ByG_Backend.src.Models
{
    public class PurchaseOrder
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = null!; // Corresponde al numeroOC
        public DateTime Date { get; set; }
        public string Status { get; set; } = "Emitida";
        public string? Observations { get; set; }
        
        // aqui hay info estática de ByG que siempre será la misma, no creo que deba ir en un model pues después se guardaría en la Base de datos información repetida siempre

        // los datos del proveedor se tomarían desde la tabla quote??

        
        // Datos de Pago y Entrega
        public string? PaymentTerms { get; set; } // ej. transferencia 30 dias
        public string? PaymentForm { get; set; } // transferencia, efectivo, etc
        public string Currency { get; set; } = "CLP";
        public DateOnly? ExpectedDeliveryDate { get; set; }
        public string? ShippingAderess { get; set; }
        public string? ShippingMethod { get; set; } // ej flete pagado, retiro en tienda
        public DateTime? DeliveryDeadline { get; set; }
        public string? CostCenter { get; set; } // C. Costo de ByG 

        // Totales de la Plantilla
        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal FreightCharge { get; set; } // Recargo por flete
        public decimal TaxExemptTotal { get; set; } // total exento 0%
        public decimal TaxRate { get; set; } // IVA 19% default
        public decimal TaxAmount { get; set; } // Cuanto es de IVA
        public decimal TotalAmount { get; set; } // Total (sin IVA)

        // Firmas y Aceptación
        public string? ApproverName { get; set; }
        public string? ApproverRole { get; set; }
        public string? ApproverRut { get; set; }
        public DateTime? SignedAt { get; set; }


        // Relaciones

        // purchaseOrder 1 a 1 purchase
        
        public int PurchaseId { get; set; }
        public Purchase Purchase { get; set; } = null!;

        // quote 1 a 1 purchaseOrder
        public int QuoteId { get; set; } // La cotización "ganadora"
        public Quote Quote { get; set; } = null!;

        // Se accede a productos a través de la Quote aceptada, allí se ven los precios
    }
}