using System;
using System.Collections.Generic;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ByG_Backend.src.Models;
using ByG_Backend.src.Options; // <- tu clase CompanyInfoOptions

namespace ByG_Backend.src.Services
{
    public class QuoteServices : IDocument
    {
        private readonly Purchase _compra;
        private readonly RequestQuote _solicitud;
        private readonly CompanyInfoOptions _company;

        // Opcional: logo
        private readonly byte[]? _logoBytes;

        // Ajusta esto para que la tabla quede como tu plantilla (cantidad de filas visibles)
        private const int TotalRowsDesired = 12;

        public QuoteServices(Purchase compra, RequestQuote solicitud, CompanyInfoOptions company, byte[]? logoBytes = null)
        {
            _compra = compra;
            _solicitud = solicitud;
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

                // Marco exterior de toda la plantilla
                page.Content()
                    .Border(1).BorderColor(Colors.Black)
                    .Padding(16)
                    .Column(root =>
                    {
                        // =========================
                        // 1) HEADER (Logo + Caja derecha)
                        // =========================
                        root.Item().Row(row =>
                        {
                            // Logo / Nombre (izquierda)
                            row.RelativeItem().Column(col =>
                            {
                                if (_logoBytes != null && _logoBytes.Length > 0)
                                {
                                    col.Item().Height(45).Image(_logoBytes);
                                }
                                else
                                {
                                    col.Item().Text(_company.BusinessName).FontSize(18).Bold();
                                }
                            });

                            // Caja derecha tipo timbre
                            row.ConstantItem(210).Border(1).BorderColor(Colors.Black).Column(col =>
                            {
                                col.Item()
                                    .Background("#D0CECE")
                                    .BorderBottom(1).BorderColor(Colors.Black)
                                    .Padding(6)
                                    .AlignCenter()
                                    .Text("SOLICITUD DE COTIZACIÓN")
                                    .FontSize(9).Bold();

                                col.Item().PaddingTop(6).AlignCenter()
                                    .Text(GetDocumentNumber())
                                    .FontSize(12).Bold().FontColor(Colors.Red.Medium);

                                col.Item().PaddingBottom(6).AlignCenter()
                                    .Text($"Fecha: {GetDocumentDate():dd-MM-yyyy}")
                                    .FontSize(8);
                            });
                        });

                        // =========================
                        // 2) BLOQUE DATOS FIJOS (Sres/Dirección/Correo/Rut/Fono)
                        // =========================
                        root.Item().PaddingTop(12)
                            .Border(1).BorderColor(Colors.Black)
                            .Padding(10)
                            .Row(row =>
                            {
                                row.ConstantItem(85).Column(left =>
                                {
                                    left.Item().Text("Sres.").Bold();
                                    left.Item().Text("Direccion").Bold();
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
                        // 4) TABLA PRINCIPAL (con filas vacías de relleno)
                        // =========================
                        root.Item().PaddingTop(14).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(40);   // ITEM
                                columns.ConstantColumn(115);  // NOMBRE
                                columns.RelativeColumn();     // DESCRIPCIÓN
                                columns.ConstantColumn(55);   // UNIDAD
                                columns.ConstantColumn(65);   // CANTIDAD
                            });

                            table.Header(h =>
                            {
                                h.Cell().Element(CellHeader).Text("ITEM");
                                h.Cell().Element(CellHeader).Text("NOMBRE");
                                h.Cell().Element(CellHeader).Text("DESCRIPCIÓN");
                                h.Cell().Element(CellHeader).Text("UNIDAD");
                                h.Cell().Element(CellHeader).Text("CANTIDAD");
                            });

                            var items = _compra.PurchaseItems?.ToList() ?? new List<PurchaseItem>();
                            int idx = 1;

                            foreach (var it in items)
                            {
                                table.Cell().Element(CellBody).AlignCenter().Text(idx.ToString());

                                // NOMBRE (Name + BrandModel + Size)
                                table.Cell().Element(CellBody).Text(t =>
                                {
                                    t.Span(it.Name ?? "").Bold();

                                    if (!string.IsNullOrWhiteSpace(it.BrandModel))
                                        t.Span($"\nMarca/Modelo: {it.BrandModel}").FontSize(8);

                                    if (!string.IsNullOrWhiteSpace(it.Size))
                                        t.Span($"\nMedida/Talla: {it.Size}").FontSize(8);
                                });

                                // DESCRIPCIÓN
                                table.Cell().Element(CellBody).Text(it.Description ?? "");

                                // UNIDAD
                                table.Cell().Element(CellBody).AlignCenter().Text(it.Unit ?? "");

                                // CANTIDAD
                                table.Cell().Element(CellBody).AlignCenter().Text(it.Quantity.ToString());

                                idx++;
                            }

                            // Relleno hasta TotalRowsDesired (para que se vea como plantilla)
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
                        // 5) OBSERVACIONES (si hay)
                        // =========================
                        if (!string.IsNullOrWhiteSpace(_compra.Observations))
                        {
                            root.Item().PaddingTop(10)
                                .Border(1).BorderColor(Colors.Black)
                                .Padding(10)
                                .Text(t =>
                                {
                                    t.Span("OBSERVACIONES: ").Bold();
                                    t.Span(_compra.Observations);
                                });
                        }

                        // =========================
                        // 6) FIRMA (centrada, como plantilla)
                        // =========================
                        root.Item().PaddingTop(22)
                            .AlignCenter()
                            .Column(firma =>
                            {
                                firma.Item().Width(260).BorderBottom(1).BorderColor(Colors.Black).PaddingBottom(3);
                                firma.Item().Text("RONALDO ZAMORANO").Bold();
                                firma.Item().Text(_company.BusinessName?.ToUpper() ?? "BYG INGENIERIA").FontSize(8);
                            });
                    });
            });
        }

        private string GetDocumentNumber()
        {
            // RFQ-2026-003 (lo principal)
            if (!string.IsNullOrWhiteSpace(_solicitud?.Number))
                return _solicitud.Number;

            // fallback: folio compra si faltara
            return _compra?.PurchaseNumber ?? "";
        }

        private DateTime GetDocumentDate()
        {
            // Evita 01-01-0001
            if (_solicitud != null && _solicitud.CreatedAt != default)
                return _solicitud.CreatedAt;

            if (_compra != null && _compra.RequestDate != default)
                return _compra.RequestDate;

            return DateTime.Now;
        }

        // ===== Estilos =====
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