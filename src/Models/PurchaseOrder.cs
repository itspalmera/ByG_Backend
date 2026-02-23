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
        public string? CostCenter { get; set; } // C. Costo de ByG 


        public string? Observations { get; set; }
        public string Status { get; set; } = "Emitida";
        
        
        // DATOS DE UNA ORDEN DE COMPRA COMPLETA EN PDF

        // Info estática de ByG que siempre será la misma, no creo que deba ir en un model pues después se guardaría en la Base de datos información repetida siempre
        // -Razon Social
        // -Direccion
        // -Rut
        // -Fono

        // Datos de proveedor (se tomarían de otra tabla, no se si Quote o Purchase)
        // -Nombre o Razón Social
        // -Dirección
        // -Rut
        // -Comuna
        // -Ciudad
        // -Fono
        // -Contacto
        
        // Datos de Pago y Entrega
        public string? PaymentForm { get; set; } // transferencia, efectivo, etc
        public string? PaymentTerms { get; set; } // ej. transferencia 30 dias
         public string Currency { get; set; } = "CLP"; // Moneda, por defecto CLP

        public DateOnly? ExpectedDeliveryDate { get; set; } // Fecha estimada de entrega, se puede calcular a partir de la fecha de la orden de compra + plazo de entrega del proveedor, pero también se puede modificar manualmente si es necesario
        public DateTime? DeliveryDeadline { get; set; } // Fecha límite de entrega, se puede calcular a partir de la fecha de la orden de compra + plazo máximo de entrega del proveedor, pero también se puede modificar manualmente si es necesario

        public string? ShippingAddress { get; set; } // Dirección de envío, puede ser diferente a la dirección del proveedor o de ByG
        public string? ShippingMethod { get; set; } // Método de envío, ej. retiro en bodega, despacho a domicilio, etc.

        

        // Totales de la Plantilla
        public decimal Discount { get; set; } // Descuento general
        public decimal FreightCharge { get; set; } // Recargo por flete

        public decimal SubTotal { get; set; } // Sub Total
        public decimal TaxExemptTotal { get; set; } // Total Exento
        public decimal TaxRate { get; set; } // numero de IVA (19% default)
        public decimal TaxAmount { get; set; } // Cuánto es de IVA (precio)

        // public decimal Sub Total con IVA //(este se calculará, pero no se guarda)
        public decimal TotalAmount { get; set; } // Total de orden de compra

        // Firmas y Aceptación
        public string? ApproverName { get; set; }
        public string? ApproverRut { get; set; }
        public string? ApproverRole { get; set; }

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