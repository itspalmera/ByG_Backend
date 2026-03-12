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
                    // DATOS EMPRESA
                    // =========================
                    root.Item().PaddingTop(12)
                    .Border(1)
                    .BorderColor(Colors.Black)
                    .Padding(10)
                    .Row(row =>
                    {
                        row.ConstantItem(85).Column(left =>
                        {
                            left.Item().Text("Empresa").Bold();
                            left.Item().Text("Dirección").Bold();
                            left.Item().Text("Correo").Bold();
                            left.Item().Text("Rut").Bold();
                            left.Item().Text("Fono").Bold();
                        });

                        row.RelativeItem().Column(right =>
                        {
                            right.Item().Text(_company.BusinessName ?? "—");
                            right.Item().Text(_company.Address ?? "—");
                            right.Item().Text(_company.Email ?? "—");
                            right.Item().Text(_company.Rut ?? "—");
                            right.Item().Text(_company.Phone ?? "—");
                        });
                    });

                    // =========================
                    // DATOS PROVEEDOR
                    // =========================
                    var supplier = _order.Quote?.Supplier;

                    root.Item().PaddingTop(10)
                    .Border(1)
                    .BorderColor(Colors.Black)
                    .Padding(10)
                    .Row(row =>
                    {
                        row.ConstantItem(85).Column(left =>
                        {
                            left.Item().Text("Proveedor").Bold();
                            left.Item().Text("Dirección").Bold();
                            left.Item().Text("Rut").Bold();
                            left.Item().Text("Fono").Bold();
                        });

                        row.RelativeItem().Column(right =>
                        {
                            right.Item().Text(supplier?.BusinessName ?? "—");
                            right.Item().Text(supplier?.Address ?? "—");
                            right.Item().Text(supplier?.Rut ?? "—");
                            right.Item().Text(supplier?.Phone ?? "—");
                        });
                    });

                    // =========================
                    // TABLA PRODUCTOS
                    // =========================
                    root.Item().PaddingTop(14).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(40); 
                            columns.RelativeColumn();  
                            columns.RelativeColumn();  
                            columns.ConstantColumn(80); 
                            columns.ConstantColumn(80); 
                        });

                        table.Header(h =>
                        {
                            h.Cell().Element(CellHeader).Text("ITEM");
                            h.Cell().Element(CellHeader).Text("MATERIAL");
                            h.Cell().Element(CellHeader).Text("DESCRIPCIÓN");
                            h.Cell().Element(CellHeader).Text("PRECIO");
                            h.Cell().Element(CellHeader).Text("TOTAL");
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
                    // TOTALES
                    // =========================
                    root.Item().PaddingTop(12)
                    .AlignRight()
                    .Column(col =>
                    {
                        col.Item().Text($"SubTotal: {_order.SubTotal:N0}");
                        col.Item().Text($"Descuento: {_order.Discount:N0}");
                        col.Item().Text($"Flete: {_order.FreightCharge:N0}");
                        col.Item().Text($"IVA ({_order.TaxRate}%): {_order.TaxAmount:N0}");

                        col.Item().Text($"TOTAL: {_order.TotalAmount:N0}")
                        .Bold()
                        .FontSize(12);
                    });

                    // =========================
                    // FIRMA
                    // =========================
                    root.Item().PaddingTop(22)
                    .AlignCenter()
                    .Column(firma =>
                    {
                        firma.Item().Width(260).BorderBottom(1).BorderColor(Colors.Black).PaddingBottom(3);
                        firma.Item().Text(_order.ApproverName ?? "").Bold();
                        firma.Item().Text(_order.ApproverRole ?? "").FontSize(8);
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