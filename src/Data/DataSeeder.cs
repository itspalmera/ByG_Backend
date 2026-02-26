using System;
using System.Collections.Generic;
using System.Linq;
using ByG_Backend.src.Data;
using ByG_Backend.src.Helpers; // Asegúrate de tener los Helpers de estados aquí
using ByG_Backend.src.Models;

namespace ByG_Backend.src.Data
{
    public static class DataSeeder
    {
        public static void Seed(DataContext context)
        {
            // Evitar duplicar datos si ya existen compras
            if (context.Purchase.Any()) return;

            // =========================================================================
            // 1. CREAR PROVEEDORES (SUPPLIERS)
            // =========================================================================
            var suppliers = new List<Supplier>
            {
                new Supplier { Rut = "76.111.222-3", BusinessName = "Materiales de Construcción El Roble", ContactName = "Juan Pérez", Email = "ventas@elroble.cl", Phone = "+56911111111", Address = "Av. Matta 123", City = "Santiago", ProductCategories = "Construcción, Obra Gruesa", IsActive = true },
                new Supplier { Rut = "77.333.444-5", BusinessName = "Sodimac Industrial", ContactName = "Ejecutivo Grandes Cuentas", Email = "contacto@sodimac.cl", Phone = "6006001234", Address = "Av. Vicuña Mackenna 1400", City = "Santiago", ProductCategories = "Construcción, Herramientas, EPP", IsActive = true },
                new Supplier { Rut = "78.555.666-7", BusinessName = "Seguridad Total Ltda", ContactName = "Maria Gonzalez", Email = "cotizaciones@seguridadtotal.cl", Phone = "+56922222222", Address = "Calle Industrial 500", City = "Antofagasta", ProductCategories = "EPP, Seguridad", IsActive = true },
                new Supplier { Rut = "79.777.888-9", BusinessName = "Ferretería O'Higgins", ContactName = "Carlos Tapia", Email = "ventas@fohiggins.cl", Phone = "+56933333333", Address = "Alameda 400", City = "Rancagua", ProductCategories = "Ferretería, Herramientas", IsActive = true },
                new Supplier { Rut = "80.999.000-1", BusinessName = "Tech Solutions", ContactName = "Ana Tech", Email = "ventas@techsolutions.cl", Phone = "+56944444444", Address = "Providencia 200", City = "Santiago", ProductCategories = "Tecnología, Oficina", IsActive = true }
            };

            context.Supplier.AddRange(suppliers);
            context.SaveChanges();

            // Referencias para usar abajo
            var supElRoble = context.Supplier.First(s => s.BusinessName.Contains("El Roble"));
            var supSodimac = context.Supplier.First(s => s.BusinessName.Contains("Sodimac"));
            var supSeguridad = context.Supplier.First(s => s.BusinessName.Contains("Seguridad Total"));
            var supFerreteria = context.Supplier.First(s => s.BusinessName.Contains("Ferretería"));

            // =========================================================================
            // ESCENARIO 1: SOLICITUD RECIÉN LLEGADA (INICIO)
            // Estado Purchase: "Solicitud recibida"
            // Estado RequestQuote: "Pendiente" (Borrador)
            // =========================================================================
            var purchase1 = new Purchase
            {
                PurchaseNumber = "REQ-2026-001",
                ProjectName = "Oficina Central",
                Status = "Solicitud recibida", // PurchaseStatuses.Received
                RequestDate = DateTime.UtcNow,
                Requester = "Secretaría",
                Observations = "Insumos mensuales de oficina",
                PurchaseItems = new List<PurchaseItem>
                {
                    new PurchaseItem { Name = "Resma Carta", Unit = "Unidad", Quantity = 50, BrandModel = "HP" },
                    new PurchaseItem { Name = "Toner Impresora", Unit = "Unidad", Quantity = 2, Description = "TN-1060" }
                }
            };
            context.Purchase.Add(purchase1);
            context.SaveChanges();

            context.RequestQuotes.Add(new RequestQuote
            {
                PurchaseId = purchase1.Id,
                Number = "RFQ-2026-001",
                Status = "Pendiente",
                CreatedAt = DateTime.UtcNow
            });
            context.SaveChanges();

            // =========================================================================
            // ESCENARIO 2: SOLICITUD ENVIADA A PROVEEDORES (ESPERANDO RESPUESTAS)
            // Estado Purchase: "Solicitud de cotización enviada"
            // Estado RequestQuote: "Enviada"
            // Sin Quotes cargadas aún.
            // =========================================================================
            var purchase2 = new Purchase
            {
                PurchaseNumber = "REQ-2026-002",
                ProjectName = "Faena Minera Norte",
                Status = "Solicitud de cotización enviada", // PurchaseStatuses.QuoteSent
                RequestDate = DateTime.UtcNow.AddDays(-2),
                Requester = "Jefe de Prevención",
                Observations = "EPP Urgente para nuevos ingresos",
                PurchaseItems = new List<PurchaseItem>
                {
                    new PurchaseItem { Name = "Casco Seguridad", Unit = "Unidad", Quantity = 20, BrandModel = "MSA" },
                    new PurchaseItem { Name = "Zapatos Seguridad", Unit = "Par", Quantity = 20, Description = "Tallas variadas" }
                }
            };
            context.Purchase.Add(purchase2);
            context.SaveChanges();

            var rq2 = new RequestQuote
            {
                PurchaseId = purchase2.Id,
                Number = "RFQ-2026-002",
                Status = "Enviada",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                SentAt = DateTime.UtcNow.AddDays(-2)
            };
            context.RequestQuotes.Add(rq2);
            context.SaveChanges();

            // Registramos que se le envió a Seguridad Total
            context.RequestQuoteSuppliers.Add(new RequestQuoteSupplier 
            { 
                RequestQuoteId = rq2.Id, 
                SupplierId = supSeguridad.Id, 
                SentAt = DateTime.UtcNow.AddDays(-2) 
            });
            context.SaveChanges();

            // =========================================================================
            // ESCENARIO 3: EN EVALUACIÓN (CON COTIZACIONES RECIBIDAS)
            // Estado Purchase: "Esperando revisión"
            // Quotes: 2 cargadas en estado "Pendiente"
            // =========================================================================
            var purchase3 = new Purchase
            {
                PurchaseNumber = "REQ-2026-003",
                ProjectName = "Edificio Centro Santiago",
                Status = "Esperando revisión", // PurchaseStatuses.WaitingReview
                RequestDate = DateTime.UtcNow.AddDays(-5),
                Requester = "Ingeniero Residente",
                Observations = "Materiales obra gruesa piso 1",
                PurchaseItems = new List<PurchaseItem>
                {
                    new PurchaseItem { Name = "Cemento Polpaico", Unit = "Saco", Quantity = 100 },
                    new PurchaseItem { Name = "Ladrillo Fiscal", Unit = "Unidad", Quantity = 500 }
                }
            };
            context.Purchase.Add(purchase3);
            context.SaveChanges();

            // Creamos la RFQ
            var rq3 = new RequestQuote { PurchaseId = purchase3.Id, Number = "RFQ-2026-003", Status = "Enviada", CreatedAt = purchase3.RequestDate, SentAt = purchase3.RequestDate };
            context.RequestQuotes.Add(rq3);
            context.SaveChanges();

            // IDs de items para enlazar precios
            var p3Items = context.PurchaseItem.Where(x => x.PurchaseId == purchase3.Id).ToList();
            var itemCemento = p3Items.First(x => x.Name.Contains("Cemento"));
            var itemLadrillo = p3Items.First(x => x.Name.Contains("Ladrillo"));

            // Quote 1: El Roble (Pendiente)
            var q3_1 = new Quote
            {
                PurchaseId = purchase3.Id,
                SupplierId = supElRoble.Id,
                Number = "COT-ROBLE-100",
                Status = "Pendiente",
                Date = DateTime.UtcNow.AddDays(-1),
                QuoteItems = new List<QuoteItem>
                {
                    new QuoteItem { Name = "Cemento Polpaico", Unit = "Saco", Quantity = 100, UnitPrice = 4500, TotalPrice = 450000, PurchaseItemId = itemCemento.Id },
                    new QuoteItem { Name = "Ladrillo Fiscal", Unit = "Unidad", Quantity = 500, UnitPrice = 350, TotalPrice = 175000, PurchaseItemId = itemLadrillo.Id }
                }
            };
            q3_1.TotalPrice = q3_1.QuoteItems.Sum(x => x.TotalPrice);

            // Quote 2: Sodimac (Pendiente)
            var q3_2 = new Quote
            {
                PurchaseId = purchase3.Id,
                SupplierId = supSodimac.Id,
                Number = "COT-SOD-500",
                Status = "Pendiente",
                Date = DateTime.UtcNow.AddDays(-1),
                QuoteItems = new List<QuoteItem>
                {
                    new QuoteItem { Name = "Cemento Melón", Unit = "Saco", Quantity = 100, UnitPrice = 4800, TotalPrice = 480000, PurchaseItemId = itemCemento.Id, Description = "Alternativa Melón" },
                    new QuoteItem { Name = "Ladrillo Fiscal", Unit = "Unidad", Quantity = 500, UnitPrice = 380, TotalPrice = 190000, PurchaseItemId = itemLadrillo.Id }
                }
            };
            q3_2.TotalPrice = q3_2.QuoteItems.Sum(x => x.TotalPrice);

            context.Quotes.AddRange(q3_1, q3_2);
            context.SaveChanges();

            // =========================================================================
            // ESCENARIO 4: OC GENERADA PERO ESPERANDO APROBACIÓN (FORMALIZADA)
            // Estado Purchase: "OC esperando aprobación" (OrderAuthorized)
            // Quote Ganadora: "Aprobada", Quote Perdedora: "Rechazada"
            // PurchaseOrder: "Esperando Aprobación" (Editable)
            // =========================================================================
            var purchase4 = new Purchase
            {
                PurchaseNumber = "REQ-2026-004",
                ProjectName = "Remodelación Baños",
                Status = "OC esperando aprobación", // PurchaseStatuses.OrderWaitingApproval
                RequestDate = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddHours(-2),
                Requester = "Arquitecto",
                PurchaseItems = new List<PurchaseItem>
                {
                    new PurchaseItem { Name = "Cerámica Blanca", Unit = "M2", Quantity = 50 },
                    new PurchaseItem { Name = "Fragüe", Unit = "Kg", Quantity = 10 }
                }
            };
            context.Purchase.Add(purchase4);
            context.SaveChanges();

            var rq4 = new RequestQuote { PurchaseId = purchase4.Id, Number = "RFQ-2026-004", Status = "Enviada", CreatedAt = purchase4.RequestDate, SentAt = purchase4.RequestDate };
            context.RequestQuotes.Add(rq4);
            context.SaveChanges();

            var p4Items = context.PurchaseItem.Where(x => x.PurchaseId == purchase4.Id).ToList();

            // Quote Ganadora (Sodimac)
            var q4_Winner = new Quote
            {
                PurchaseId = purchase4.Id,
                SupplierId = supSodimac.Id,
                Number = "COT-SOD-700",
                Status = "Aprobada", // Aprobada
                Date = DateTime.UtcNow.AddDays(-3),
                QuoteItems = new List<QuoteItem>
                {
                    new QuoteItem { Name = "Cerámica", Quantity = 50, Unit = "M2", UnitPrice = 8000, TotalPrice = 400000, PurchaseItemId = p4Items[0].Id },
                    new QuoteItem { Name = "Fragüe", Quantity = 10, Unit = "Kg", UnitPrice = 1500, TotalPrice = 15000, PurchaseItemId = p4Items[1].Id }
                }
            };
            q4_Winner.TotalPrice = 415000;

            // Quote Perdedora (El Roble)
            var q4_Loser = new Quote
            {
                PurchaseId = purchase4.Id,
                SupplierId = supElRoble.Id,
                Number = "COT-ROB-200",
                Status = "Rechazada", // Rechazada
                Date = DateTime.UtcNow.AddDays(-3),
                TotalPrice = 450000
            };

            context.Quotes.AddRange(q4_Winner, q4_Loser);
            context.SaveChanges();

            // CREAR LA ORDEN DE COMPRA (ESTADO: ESPERANDO APROBACIÓN)
            var po4 = new PurchaseOrder
            {
                PurchaseId = purchase4.Id,
                QuoteId = q4_Winner.Id,
                OrderNumber = "OC-2026-0001",
                Date = DateTime.UtcNow.AddHours(-2),
                Status = "Esperando Aprobación", // PurchaseOrderStatuses.WaitingApproval
                
                // Datos de formalización
                CostCenter = "CC-REMODELACION-01",
                PaymentForm = "Transferencia",
                PaymentTerms = "30 días",
                ShippingAddress = "Calle La Obra 555, Santiago",
                ShippingMethod = "Despacho a obra",
                Currency = "CLP",
                
                // Cálculos
                SubTotal = 415000,
                Discount = 0,
                FreightCharge = 15000, // Flete agregado en formalización
                TaxRate = 19,
                TaxAmount = (415000 + 15000) * 0.19m, // 81700
                TotalAmount = 430000 * 1.19m, // 511700

                ApproverName = "Admin Inicial",
                ApproverRole = "Gestor",
                SignedAt = null // Aún no firmada
            };
            context.PurchaseOrder.Add(po4);
            context.SaveChanges();


            // =========================================================================
            // ESCENARIO 5: OC FINALIZADA Y ENVIADA (CICLO COMPLETO)
            // Estado Purchase: "OC enviada" (OrderSent)
            // PurchaseOrder: "Enviada" (Firmada)
            // =========================================================================
            var purchase5 = new Purchase
            {
                PurchaseNumber = "REQ-2026-005",
                ProjectName = "Mantención Maquinaria",
                Status = "OC enviada", // PurchaseStatuses.OrderSent
                RequestDate = DateTime.UtcNow.AddDays(-20),
                UpdatedAt = DateTime.UtcNow.AddDays(-2),
                Requester = "Jefe Taller",
                PurchaseItems = new List<PurchaseItem>
                {
                    new PurchaseItem { Name = "Aceite Motor", Unit = "Litro", Quantity = 20 }
                }
            };
            context.Purchase.Add(purchase5);
            context.SaveChanges();

            var rq5 = new RequestQuote { PurchaseId = purchase5.Id, Number = "RFQ-2026-005", Status = "Cerrada", CreatedAt = purchase5.RequestDate, SentAt = purchase5.RequestDate };
            context.RequestQuotes.Add(rq5);
            context.SaveChanges();

            var p5Item = context.PurchaseItem.First(x => x.PurchaseId == purchase5.Id);

            var q5_Winner = new Quote
            {
                PurchaseId = purchase5.Id,
                SupplierId = supFerreteria.Id,
                Number = "COT-FER-900",
                Status = "Aprobada",
                TotalPrice = 100000,
                QuoteItems = new List<QuoteItem> { new QuoteItem { Name = "Aceite Motor", Quantity = 20, Unit = "Litro", UnitPrice = 5000, TotalPrice = 100000, PurchaseItemId = p5Item.Id } }
            };
            context.Quotes.Add(q5_Winner);
            context.SaveChanges();

            var po5 = new PurchaseOrder
            {
                PurchaseId = purchase5.Id,
                QuoteId = q5_Winner.Id,
                OrderNumber = "OC-2026-0002",
                Date = DateTime.UtcNow.AddDays(-2),
                Status = "Enviada", // PurchaseOrderStatuses.Sent
                
                CostCenter = "CC-MAQUINARIA",
                PaymentForm = "Efectivo",
                PaymentTerms = "Contra entrega",
                ShippingAddress = "Taller Central",
                ShippingMethod = "Retiro en tienda",
                
                SubTotal = 100000,
                TaxRate = 19,
                TaxAmount = 19000,
                TotalAmount = 119000,

                ApproverName = "Gerente Operaciones",
                ApproverRole = "Gerencia",
                ApproverRut = "11.222.333-K",
                SignedAt = DateTime.UtcNow.AddDays(-2) // Firmada
            };
            context.PurchaseOrder.Add(po5);
            context.SaveChanges();
        }
    }
}