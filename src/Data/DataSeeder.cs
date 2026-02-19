using System;
using System.Collections.Generic;
using System.Linq;
using ByG_Backend.src.Data;
using ByG_Backend.src.Helpers;
using ByG_Backend.src.Models;

namespace ByG_Backend.src.Data
{
    public static class DataSeeder
    {
        public static void Seed(DataContext context)
        {
            // Evitar duplicar datos si ya existen compras (para no ensuciar la BD en cada reinicio)
            if (context.Purchase.Any()) return;

            // =========================================================================
            // 1. CREAR PROVEEDORES (SUPPLIERS)
            // Estos son los actores que enviarán las cotizaciones.
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
            context.SaveChanges(); // Guardamos para generar los IDs reales en la BD

            // Recuperamos las entidades con sus IDs generados para relacionarlas correctamente abajo
            var supElRoble = context.Supplier.First(s => s.BusinessName.Contains("El Roble"));
            var supSodimac = context.Supplier.First(s => s.BusinessName.Contains("Sodimac"));
            var supSeguridad = context.Supplier.First(s => s.BusinessName.Contains("Seguridad Total"));

            // =========================================================================
            // 2. ESCENARIO A: COMPRA DE MATERIALES (CON COTIZACIONES CARGADAS)
            // Estado: WaitingReview (El gestor ya subió cotizaciones y espera al autorizador)
            // =========================================================================
            
            // 2.1 Crear la Necesidad (Purchase) con sus Items
            var purchaseA = new Purchase
            {
                PurchaseNumber = "REQ-2026-001",
                ProjectName = "Edificio Centro Santiago",
                Status = PurchaseStatuses.WaitingReview, // Estado avanzado
                RequestDate = DateTime.UtcNow.AddDays(-5),
                Requester = "Ingeniero Residente",
                Observations = "Materiales urgentes para obra gruesa piso 1",
                PurchaseItems = new List<PurchaseItem>
                {
                    // Estos son los productos "Padre" que se solicitan
                    new PurchaseItem { Name = "Cemento Polpaico", BrandModel = "Especial", Unit = "Saco", Quantity = 100, Description = "Saco de 25kg" },
                    new PurchaseItem { Name = "Arena Rubia", BrandModel = "N/A", Unit = "M3", Quantity = 10, Description = "Arena limpia" },
                    new PurchaseItem { Name = "Ladrillo Fiscal", BrandModel = "Standard", Unit = "Unidad", Quantity = 500, Description = "7x14x28" }
                }
            };

            context.Purchase.Add(purchaseA);
            context.SaveChanges(); // Guardar para obtener IDs de PurchaseItems vitales para vincular la cotización

            // 2.2 Crear el registro de que se enviaron correos (RequestQuote)
            var requestQuoteA = new RequestQuote
            {
                PurchaseId = purchaseA.Id,
                Number = "RFQ-2026-001",
                Status = "Enviada",
                CreatedAt = purchaseA.RequestDate,
                SentAt = purchaseA.RequestDate.AddHours(2)
            };
            context.RequestQuotes.Add(requestQuoteA);
            context.SaveChanges();

            // Relacionar a qué proveedores se les envió el correo
            context.RequestQuoteSuppliers.AddRange(
                new RequestQuoteSupplier { RequestQuoteId = requestQuoteA.Id, SupplierId = supElRoble.Id, SentAt = DateTime.UtcNow },
                new RequestQuoteSupplier { RequestQuoteId = requestQuoteA.Id, SupplierId = supSodimac.Id, SentAt = DateTime.UtcNow }
            );

            // 2.3 CREAR COTIZACIONES (QUOTES)
            // Aquí aseguramos que cada cotización tenga un Proveedor y sus precios enlazados al item original.
            
            // Recuperamos los items originales de la compra para obtener sus IDs
            var itemsA = context.PurchaseItem.Where(x => x.PurchaseId == purchaseA.Id).ToList();
            var itemCemento = itemsA.First(x => x.Name.Contains("Cemento"));
            var itemArena = itemsA.First(x => x.Name.Contains("Arena"));
            var itemLadrillo = itemsA.First(x => x.Name.Contains("Ladrillo"));

            // --- Cotización 1: El Roble ---
            var quote1 = new Quote
            {
                PurchaseId = purchaseA.Id,
                SupplierId = supElRoble.Id, // <--- ¡AQUÍ! Asignación obligatoria del proveedor
                Number = "COT-ROBLE-001",
                Status = "Pendiente", 
                Date = DateTime.UtcNow.AddDays(-2),
                QuoteItems = new List<QuoteItem>
                {
                    // Enlazamos precio con el ID del producto solicitado (PurchaseItemId)
                    new QuoteItem { Name = "Cemento Polpaico", Unit = "Saco", Quantity = 100, UnitPrice = 4500, TotalPrice = 450000, PurchaseItemId = itemCemento.Id },
                    new QuoteItem { Name = "Arena Rubia", Unit = "M3", Quantity = 10, UnitPrice = 18000, TotalPrice = 180000, PurchaseItemId = itemArena.Id },
                    new QuoteItem { Name = "Ladrillo Fiscal", Unit = "Unidad", Quantity = 500, UnitPrice = 350, TotalPrice = 175000, PurchaseItemId = itemLadrillo.Id }
                }
            };
            quote1.TotalPrice = quote1.QuoteItems.Sum(x => x.TotalPrice);

            // --- Cotización 2: Sodimac ---
            var quote2 = new Quote
            {
                PurchaseId = purchaseA.Id,
                SupplierId = supSodimac.Id, // <--- ¡AQUÍ! Asignación obligatoria del proveedor
                Number = "COT-SOD-999",
                Status = "Pendiente",
                Date = DateTime.UtcNow.AddDays(-1),
                QuoteItems = new List<QuoteItem>
                {
                    new QuoteItem { Name = "Cemento Melón", Unit = "Saco", Quantity = 100, UnitPrice = 4800, TotalPrice = 480000, PurchaseItemId = itemCemento.Id, Description = "Solo tenían Melón (Alternativo)" },
                    new QuoteItem { Name = "Arena Rubia", Unit = "M3", Quantity = 10, UnitPrice = 17500, TotalPrice = 175000, PurchaseItemId = itemArena.Id },
                    new QuoteItem { Name = "Ladrillo Fiscal", Unit = "Unidad", Quantity = 500, UnitPrice = 380, TotalPrice = 190000, PurchaseItemId = itemLadrillo.Id }
                }
            };
            quote2.TotalPrice = quote2.QuoteItems.Sum(x => x.TotalPrice);

            context.Quotes.AddRange(quote1, quote2);

            // =========================================================================
            // 3. ESCENARIO B: COMPRA DE EPP (SOLO SOLICITUD ENVIADA)
            // Estado: QuoteSent (Se enviaron correos, pero el Gestor aún no carga precios)
            // =========================================================================

            var purchaseB = new Purchase
            {
                PurchaseNumber = "REQ-2026-002",
                ProjectName = "Faena Minera Norte",
                Status = PurchaseStatuses.QuoteSent,
                RequestDate = DateTime.UtcNow.AddDays(-1),
                Requester = "Jefe de Prevención",
                Observations = "EPP para nuevos ingresos",
                PurchaseItems = new List<PurchaseItem>
                {
                    new PurchaseItem { Name = "Casco Seguridad", BrandModel = "MSA", Unit = "Unidad", Quantity = 20, Description = "Color Blanco tipo V-Gard" },
                    new PurchaseItem { Name = "Lentes Seguridad", BrandModel = "3M", Unit = "Par", Quantity = 20, Description = "Spyder oscuros" },
                    new PurchaseItem { Name = "Zapatos Seguridad", BrandModel = "Nazca", Unit = "Par", Quantity = 20, Description = "Tallas variadas" }
                }
            };

            context.Purchase.Add(purchaseB);
            context.SaveChanges();

            var requestQuoteB = new RequestQuote
            {
                PurchaseId = purchaseB.Id,
                Number = "RFQ-2026-002",
                Status = "Enviada",
                CreatedAt = DateTime.UtcNow,
                SentAt = DateTime.UtcNow
            };
            context.RequestQuotes.Add(requestQuoteB);
            context.SaveChanges();

            // Solo registramos que se le pidió a "Seguridad Total", pero aún no hay Quote creada
            context.RequestQuoteSuppliers.Add(new RequestQuoteSupplier { RequestQuoteId = requestQuoteB.Id, SupplierId = supSeguridad.Id, SentAt = DateTime.UtcNow });


            // =========================================================================
            // 4. ESCENARIO C: COMPRA DE INSUMOS (NUEVA)
            // Estado: Received (Nadie ha hecho nada aún)
            // =========================================================================
            
            var purchaseC = new Purchase
            {
                PurchaseNumber = "REQ-2026-003",
                ProjectName = "Oficina Central",
                Status = PurchaseStatuses.Received,
                RequestDate = DateTime.UtcNow,
                Requester = "Secretaría",
                Observations = "Insumos mensuales",
                PurchaseItems = new List<PurchaseItem>
                {
                    new PurchaseItem { Name = "Resma Carta", BrandModel = "HP", Unit = "Unidad", Quantity = 50 },
                    new PurchaseItem { Name = "Toner Impresora", BrandModel = "Brother", Unit = "Unidad", Quantity = 2, Description = "TN-1060" }
                }
            };

            context.Purchase.Add(purchaseC);
            context.SaveChanges();

            // Solicitud creada automáticamente, pero en estado pendiente (borrador)
            var requestQuoteC = new RequestQuote
            {
                PurchaseId = purchaseC.Id,
                Number = "RFQ-2026-003",
                Status = "Pendiente",
                CreatedAt = DateTime.UtcNow
            };
            context.RequestQuotes.Add(requestQuoteC);
            
            // Guardamos todo lo pendiente
            context.SaveChanges();
        }
    }
}