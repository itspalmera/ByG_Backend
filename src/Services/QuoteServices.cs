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
    /// <summary>
    /// Servicio encargado de la generación de documentos PDF para solicitudes de cotización.
    /// Implementa <see cref="IDocument"/> de QuestPDF para definir la estructura visual,
    /// encabezados, tablas de ítems y firmas del documento.
    /// </summary>
    public class QuoteServices : IDocument
    {
        private readonly Purchase _compra;
        private readonly RequestQuote _solicitud;
        private readonly CompanyInfoOptions _company;
        private readonly byte[]? _logoBytes;

        /// <summary>
        /// Cantidad de filas mínimas que se mostrarán en la tabla de ítems para mantener la estética de la plantilla.
        /// </summary>
        private const int TotalRowsDesired = 12;

        /// <summary>
        /// Inicializa una nueva instancia del generador de documentos.
        /// </summary>
        /// <param name="compra">Modelo de la compra con los ítems a cotizar.</param>
        /// <param name="solicitud">Modelo de la solicitud que contiene el número de RFQ y fechas.</param>
        /// <param name="company">Opciones con la información legal y de contacto de la empresa.</param>
        /// <param name="logoBytes">Opcional: Arreglo de bytes de la imagen del logo corporativo.</param>
        public QuoteServices(Purchase compra, RequestQuote solicitud, CompanyInfoOptions company, byte[]? logoBytes = null)
        {
            _compra = compra;
            _solicitud = solicitud;
            _company = company;
            _logoBytes = logoBytes;
        }

        /// <summary>
        /// Define los metadatos del documento PDF generado.
        /// </summary>
        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        /// <summary>
        /// Orquestador principal de la composición del PDF.
        /// Define el tamaño de página (A4), márgenes, tipografía y estructura por bloques.
        /// </summary>
        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.0f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Arial));

                // Marco exterior y contenedor principal
                page.Content()
                    .Border(1).BorderColor(Colors.Black)
                    .Padding(16)
                    .Column(root =>
                    {
                        // 1) HEADER: Logo o Nombre de Empresa y Cuadro de Título del Documento
                        root.Item().Row(row =>
                        {
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

                            // Cuadro de identificación del documento (Folio y Fecha)
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

                        // 2) DATOS DE LA EMPRESA: Información de contacto y RUT
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

                        // 3) TABLA DE ÍTEMS: Listado de productos con celdas de relleno
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

                            // Renderizado de ítems reales
                            foreach (var it in items)
                            {
                                table.Cell().Element(CellBody).AlignCenter().Text(idx.ToString());

                                table.Cell().Element(CellBody).Text(t =>
                                {
                                    t.Span(it.Name ?? "").Bold();
                                    if (!string.IsNullOrWhiteSpace(it.BrandModel))
                                        t.Span($"\nMarca/Modelo: {it.BrandModel}").FontSize(8);
                                    if (!string.IsNullOrWhiteSpace(it.Size))
                                        t.Span($"\nMedida/Talla: {it.Size}").FontSize(8);
                                });

                                table.Cell().Element(CellBody).Text(it.Description ?? "");
                                table.Cell().Element(CellBody).AlignCenter().Text(it.Unit ?? "");
                                table.Cell().Element(CellBody).AlignCenter().Text(it.Quantity.ToString());
                                idx++;
                            }

                            // Renderizado de filas vacías para completar la plantilla visual
                            for (int k = items.Count; k < TotalRowsDesired; k++)
                            {
                                table.Cell().Element(CellBody).Text("");
                                table.Cell().Element(CellBody).Text("");
                                table.Cell().Element(CellBody).Text("");
                                table.Cell().Element(CellBody).Text("");
                                table.Cell().Element(CellBody).Text("");
                            }
                        });

                        // 4) OBSERVACIONES: Sección condicional
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

                        // 5) SECCIÓN DE FIRMA: Autorización del documento
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

        /// <summary>
        /// Determina el número de documento priorizando el número de solicitud (RFQ).
        /// </summary>
        private string GetDocumentNumber()
        {
            if (!string.IsNullOrWhiteSpace(_solicitud?.Number))
                return _solicitud.Number;
            return _compra?.PurchaseNumber ?? "";
        }

        /// <summary>
        /// Obtiene la fecha de emisión del documento basándose en la creación de la solicitud o la compra.
        /// </summary>
        private DateTime GetDocumentDate()
        {
            if (_solicitud != null && _solicitud.CreatedAt != default)
                return _solicitud.CreatedAt;
            if (_compra != null && _compra.RequestDate != default)
                return _compra.RequestDate;
            return DateTime.Now;
        }

        // ===== Helpers de Estilo de Celdas =====

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