using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

using ByG_Backend.src.Models;


namespace ByG_Backend.src.Services
{
    public class QuoteServices : IDocument
    {
        private readonly Purchase _compra;
        private readonly RequestQuote _solicitud;
        public QuoteServices(Purchase compra, RequestQuote solicitud)
        {
            _compra = compra;
            _solicitud = solicitud;
        }

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(1, Unit.Centimetre);
                page.PageColor(Colors.White);
                
               // CABECERA con datos fijos de ByG Ingeniería 
                page.Header().Row(row => {
                    row.RelativeItem().Column(col => {
                        col.Item().Text("ByG Ingeniería").FontSize(20).Bold().FontColor(Colors.Blue.Medium);
                        col.Item().Text("RUT: 76.346.990-5").FontSize(9);
                        col.Item().Text("Román Díaz N°205 dpto 102").FontSize(9);
                        col.Item().Text("Fono: 2-22343993").FontSize(9);
                    });
                    
                    row.ConstantItem(150).Border(0.5f).Column(col => {
                        col.Item().AlignCenter().Background(Colors.Grey.Lighten3).Text("SOLICITUD DE COTIZACIÓN").FontSize(8).Bold();
                        col.Item().AlignCenter().PaddingVertical(5).Text(_solicitud.Number).FontSize(14).Bold(); // [cite: 261]
                    });
                });

                page.Content().PaddingVertical(10).Column(col => {
                   // Información del Proyecto [cite: 117]
                    col.Item().PaddingBottom(10).Text(t => {
                        t.Span("Proyecto: ").Bold();
                        t.Span(_compra.ProjectName);
                    });
                    
                    col.Item().Table(table => {
                        table.ColumnsDefinition(c => {
                            c.ConstantColumn(30);  // ITEM (Iterativo)
                            c.RelativeColumn(3);   // Nombre
                            c.RelativeColumn(4);   // Descripción/Marca [cite: 138, 139]
                            c.ConstantColumn(50);  // Cantidad 
                            c.ConstantColumn(50);  // Unidad [cite: 140]
                        });
                        
                        table.Header(h => {
                            h.Cell().Element(Style).AlignCenter().Text("ITEM");
                            h.Cell().Element(Style).Text("NOMBRE");
                            h.Cell().Element(Style).Text("DETALLE");
                            h.Cell().Element(Style).AlignCenter().Text("CANT.");
                            h.Cell().Element(Style).AlignCenter().Text("UNID.");

                            static IContainer Style(IContainer container) => container.BorderBottom(1).PaddingVertical(2).DefaultTextStyle(x => x.Bold().FontSize(9));
                        });
                        
                        // --- AQUÍ OCURRE LA ITERACIÓN QUE SOLICITASTE ---
                        int i = 1;
                        foreach (var producto in _compra.PurchaseItems) // [cite: 126, 135]
                        {
                            table.Cell().Element(CellS).AlignCenter().Text(i.ToString()); // 1, 2, 3...
                            table.Cell().Element(CellS).Text(producto.Name);
                            table.Cell().Element(CellS).Text($"{producto.BrandModel} {producto.Description}");
                            table.Cell().Element(CellS).AlignCenter().Text(producto.Quantity.ToString());
                            table.Cell().Element(CellS).AlignCenter().Text(producto.Unit);
                            
                            i++; // Incremento para la siguiente fila
                        }

                        static IContainer CellS(IContainer container) => container.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(2).DefaultTextStyle(x => x.FontSize(9));
                    });
                });

               // PIE DE PÁGINA con firma de Ronaldo Zamorano [cite: 327]
                page.Footer().AlignRight().Column(f => {
                    f.Item().PaddingTop(30).AlignCenter().Width(150).BorderTop(0.5f).Column(c => {
                        c.Item().Text("RONALDO ZAMORANO").FontSize(9).Bold().AlignCenter();
                        c.Item().Text("BYG INGENIERIA").FontSize(8).AlignCenter();
                    });
                });
            });
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
    }
}