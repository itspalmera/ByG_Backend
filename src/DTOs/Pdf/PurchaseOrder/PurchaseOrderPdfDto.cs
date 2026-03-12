using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.DTOs
{
    /// <summary>
    /// DTO compuesto que agrupa toda la información necesaria para generar el reporte PDF de una Orden de Compra.
    /// Actúa como el modelo de datos raíz para las plantillas de visualización y exportación.
    /// </summary>
    public class PurchaseOrderPdfDto
    {
        /// <summary>
        /// Contiene los datos administrativos y financieros de la Orden (folios, fechas, totales e impuestos).
        /// </summary>
        public PdfPurchaseOrderData Order { get; set; } = null!;

        /// <summary>
        /// Contiene la información legal y de contacto del proveedor seleccionado para esta orden.
        /// </summary>
        public PdfSupplierData Supplier { get; set; } = null!;

        /// <summary>
        /// Lista detallada de todos los productos o servicios que componen la transacción, 
        /// incluyendo sus especificaciones técnicas y precios unitarios.
        /// </summary>
        public List<PdfQuoteItemData> Items { get; set; } = new();
    }
}