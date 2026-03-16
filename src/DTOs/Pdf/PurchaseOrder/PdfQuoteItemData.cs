using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.DTOs
{
    /// <summary>
    /// Representa el detalle individual de un producto o servicio dentro de un reporte PDF.
    /// Diseñado para ser mapeado directamente a las filas de la tabla de ítems en el documento impreso.
    /// </summary>
    public class PdfQuoteItemData
    {
        /// <summary>
        /// Descripción detallada o nombre del producto que se visualizará en la columna principal del PDF.
        /// </summary>
        public string Description { get; set; } = null!;

        /// <summary>
        /// Cantidad de unidades solicitadas o cotizadas.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Unidad de medida (ej: "kg", "unidad", "metro"). Proporciona contexto a la cantidad.
        /// </summary>
        public string Unit { get; set; } = null!;

        /// <summary>
        /// Marca específica del producto. Ayuda a garantizar que se entregue el material correcto en terreno.
        /// </summary>
        public string? Brand { get; set; }

        /// <summary>
        /// Modelo o referencia técnica del fabricante para el ítem.
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// Valor monetario por unidad, formateado según la moneda de la orden.
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Monto total de la línea (usualmente Quantity * UnitPrice).
        /// </summary>
        public decimal Total { get; set; } 
    }
}