using System;
using System.Collections.Generic;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ByG_Backend.src.Models;
using ByG_Backend.src.Options;

namespace ByG_Backend.src.Services
{
    public class PurchaseOrderServices : IDocument
    {
        private readonly PurchaseOrder _order;
        private readonly CompanyInfoOptions _company;
        private readonly byte[]? _logoBytes;

        private const int TotalRowsDesired = 12;

        public PurchaseOrderServices(PurchaseOrder order, CompanyInfoOptions company, byte[]? logoBytes = null)
        {
            _order = order;
            _company = company;
            _logoBytes = logoBytes;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.0f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Arial));

                page.Content()
                .Border(1)
                .BorderColor(Colors.Black)
                .Padding(16)
                .Column(root =>
                {

                    // =========================
                    // HEADER
                    // =========================
                    root.Item().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            if (_logoBytes != null && _logoBytes.Length > 0)
                                col.Item().Height(45).Image(_logoBytes);
                            else
                                col.Item().Text(_company.BusinessName).FontSize(18).Bold();
                        });

                        row.ConstantItem(210)
                        .Border(1)
                        .BorderColor(Colors.Black)
                        .Column(col =>
                        {
                            col.Item()
                            .Background("#D0CECE")
                            .BorderBottom(1)
                            .BorderColor(Colors.Black)
                            .Padding(6)
                            .AlignCenter()
                            .Text("ORDEN DE COMPRA")
                            .Bold();

                            col.Item().PaddingTop(6)
                            .AlignCenter()
                            .Text(_order.OrderNumber)
                            .FontSize(12)
                            .Bold()
                            .FontColor(Colors.Red.Medium);

                            col.Item().PaddingBottom(6)
                            .AlignCenter()
                            .Text($"Fecha: {_order.Date:dd-MM-yyyy}")
                            .FontSize(8);
                        });
                    });




                    // =========================
                    // BLOQUE SUPERIOR (IZQ: Empresa + Proveedor | DER: Orden)
                    // =========================
                    root.Item().PaddingTop(12).Row(mainRow =>
                    {
                        // ================= IZQUIERDA =================
                        mainRow.RelativeItem(3).Column(left =>
                        {
                            // EMPRESA
                            left.Item()
                            .Border(1).BorderColor(Colors.Black)
                            .Padding(10)
                            .Row(row =>
                            {
                                row.ConstantItem(45).Column(col =>
                                {
                                    col.Item().Text("Empresa").Bold();
                                    col.Item().Text("Dirección").Bold();
                                    col.Item().Text("Correo").Bold();
                                    col.Item().Text("Rut").Bold();
                                    col.Item().Text("Fono").Bold();
                                });

                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text(_company.BusinessName ?? "—");
                                    col.Item().Text(_company.Address ?? "—");
                                    col.Item().Text(_company.Email ?? "—");
                                    col.Item().Text(_company.Rut ?? "—");
                                    col.Item().Text(_company.Phone ?? "—");
                                });
                            });

                            // PROVEEDOR
                            var supplier = _order.Quote?.Supplier;

                            left.Item().PaddingTop(10)
                            .Border(1).BorderColor(Colors.Black)
                            .Padding(10)
                            .Row(row =>
                            {
                                row.ConstantItem(45).Column(col =>
                                {
                                    col.Item().Text("Proveedor").Bold();
                                    col.Item().Text("Dirección").Bold();
                                    col.Item().Text("Rut").Bold();
                                    col.Item().Text("Fono").Bold();
                                });

                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text(supplier?.BusinessName ?? "—");
                                    col.Item().Text(supplier?.Address ?? "—");
                                    col.Item().Text(supplier?.Rut ?? "—");
                                    col.Item().Text(supplier?.Phone ?? "—");
                                });
                            });
                        });

                        // ================= DERECHA =================
                        mainRow.RelativeItem(2).PaddingLeft(3)
                        .Border(1).BorderColor(Colors.Black)
                        .Padding(6)
                        .Column(col =>
                        {
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("Centro de costo").Bold();
                                    c.Item().Text("Estado").Bold();
                                    c.Item().Text("Medio de pago").Bold();
                                    c.Item().Text("Condición de pago").Bold();
                                    c.Item().Text("Moneda").Bold();
                                });

                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text(_order.CostCenter ?? "—");
                                    c.Item().Text(_order.Status ?? "—");
                                    c.Item().Text(_order.PaymentForm ?? "—");
                                    c.Item().Text(_order.PaymentTerms ?? "—");
                                    c.Item().Text(_order.Currency ?? "—");
                                });
                            });

                            col.Item().PaddingTop(10).Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("Plazo entrega").Bold();
                                    c.Item().Text("Plazo máx. entrega").Bold();
                                    c.Item().Text("Dirección envío").Bold();
                                    c.Item().Text("Método envío").Bold();
                                });

                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text(_order.ExpectedDeliveryDate?.ToString("dd-MM-yyyy") ?? "—");
                                    c.Item().Text(_order.DeliveryDeadline?.ToString("dd-MM-yyyy") ?? "—");
                                    c.Item().Text(_order.ShippingAddress ?? "—");
                                    c.Item().Text(_order.ShippingMethod ?? "—");
                                });
                            });
                        });
                    });


                    // =========================
                    // TABLA PRODUCTOS
                    // =========================
                    root.Item().PaddingTop(14).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(40); // ITEM
                            columns.RelativeColumn();   // MATERIAL
                            columns.RelativeColumn();   // DESCRIPCIÓN
                            columns.ConstantColumn(40); // UNIDAD
                            columns.ConstantColumn(40); // CANTIDAD
                            columns.ConstantColumn(50); // UNITARIO
                            columns.ConstantColumn(80); // TOTAL
                        });

                        table.Header(h =>
                        {
                            h.Cell().Element(CellHeader).Text("Ítem");
                            h.Cell().Element(CellHeader).Text("Material");
                            h.Cell().Element(CellHeader).Text("Descripción");
                            h.Cell().Element(CellHeader).Text("Unid.");
                            h.Cell().Element(CellHeader).Text("Cant.");
                            h.Cell().Element(CellHeader).Text("Precio");
                            h.Cell().Element(CellHeader).Text("Total");
                        });

                        var items = _order.Quote?.QuoteItems ?? new List<QuoteItem>();

                        int idx = 1;

                        foreach (var item in items)
                        {
                            var price = item.UnitPrice ?? 0;
                            var total = item.Quantity * price;

                            table.Cell().Element(CellBody).AlignCenter().Text(idx.ToString());
                            table.Cell().Element(CellBody).Text(item.Name ?? "");
                            table.Cell().Element(CellBody).Text(item.Description ?? "");
                            table.Cell().Element(CellBody).AlignCenter().Text(item.Unit ?? "");
                            table.Cell().Element(CellBody).AlignRight().Text(item.Quantity.ToString());
                            table.Cell().Element(CellBody).AlignRight().Text(price.ToString("N0"));
                            table.Cell().Element(CellBody).AlignRight().Text(total.ToString("N0"));

                            idx++;
                        }

                        for (int k = items.Count; k < TotalRowsDesired; k++)
                        {
                            table.Cell().Element(CellBody).Text("");
                            table.Cell().Element(CellBody).Text("");
                            table.Cell().Element(CellBody).Text("");
                            table.Cell().Element(CellBody).Text("");
                            table.Cell().Element(CellBody).Text("");
                        }
                    });

                    // =========================
                    // TOTALES Y OBSERVACIONES
                    // =========================
                    root.Item().PaddingTop(12)
                    .Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2); // Observaciones
                            columns.RelativeColumn(1); // Totales
                        });

                        table.Cell().Border(1).Padding(8).Column(obs =>
                        {
                            obs.Item().Text("Observaciones").Bold();
                            obs.Item().Height(80).Text(_order.Observations ?? "");
                        });

                        table.Cell().Border(1).Padding(4).Column(tot =>
                        {
                            tot.Item().Text($"Valor compra: {_order.SubTotal:N0}");
                            tot.Item().Text($"Descuento: {_order.Discount:N0}");
                            tot.Item().Text($"Recargo flete: {_order.FreightCharge:N0}");
                            tot.Item().Text($"SubTotal: {_order.SubTotal:N0}");
                            tot.Item().Text($"Total Exento: {_order.TaxExemptTotal:N0}");
                            tot.Item().Text($"IVA ({_order.TaxRate}%): {_order.TaxAmount:N0}");
                            tot.Item().Text($"SubTotal c/IVA: {_order.TotalAmount:N0}");
                            tot.Item().Text($"Retención: 0"); // si no tienes campo aún

                            tot.Item().PaddingTop(5)
                            .Text($"TOTAL: {_order.TotalAmount:N0}")
                            .Bold()
                            .FontSize(10);
                        });
                    });

                    // =========================
                    // FIRMA
                    // =========================
                    root.Item().PaddingTop(10)
                    .Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                        });

                        // DATOS ACEPTANTE
                        table.Cell().Border(1).Padding(5).Column(col =>
                        {
                            col.Item().Text("Nombre Aceptante").Bold();
                            col.Item().Text(_order.ApproverName ?? "");

                            col.Item().PaddingTop(5).Text("RUT").Bold();
                            col.Item().Text(_order.ApproverRut ?? "");

                            col.Item().PaddingTop(5).Text("Cargo").Bold();
                            col.Item().Text(_order.ApproverRole ?? "");

                            col.Item().PaddingTop(5).Text("Fecha").Bold();
                            col.Item().Text(_order.SignedAt?.ToString("dd-MM-yyyy") ?? "");
                        });

                        // FIRMA ACEPTANTE
                        table.Cell().Border(1).Padding(8).Column(col =>
                        {
                            col.Item().Text("FIRMA ACEPTANTE").Bold();
                            col.Item().Height(80);
                        });

                        // FIRMA EMPRESA
                        table.Cell().Border(1).Padding(8).Column(col =>
                        {
                            col.Item().Text("FIRMA").Bold();
                            col.Item().Height(80);
                        });
                    });
                });
            });
        }

        private static IContainer CellHeader(IContainer c) =>
            c.Border(1).BorderColor(Colors.Black)
             .Background("#D0CECE")
             .PaddingVertical(6).PaddingHorizontal(6)
             .AlignCenter().AlignMiddle()
             .DefaultTextStyle(x => x.Bold());

        private static IContainer CellBody(IContainer c) =>
            c.Border(1).BorderColor(Colors.Black)
             .PaddingVertical(8).PaddingHorizontal(6)
             .AlignMiddle();
    }
}