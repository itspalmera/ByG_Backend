using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.Models
{
    /// <summary>
    /// Representa una Cotización formal emitida por un proveedor.
    /// Es el documento donde se registran los precios y condiciones ofrecidas en respuesta 
    /// a un requerimiento de compra interno.
    /// </summary>
    public class Quote
    {
        /// <summary>
        /// Identificador único autoincremental de la cotización en la base de datos.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Número de folio o referencia externa de la cotización (ej: COT-9923).
        /// Es el identificador que el proveedor asigna a su oferta.
        /// </summary>
        public required string Number { get; set; }

        /// <summary>
        /// Estado actual de la cotización (ej: "Pendiente", "Aceptada", "Rechazada").
        /// Determina si la oferta puede ser utilizada para generar una Orden de Compra.
        /// </summary>
        public required string Status { get; set; }

        /// <summary>
        /// Fecha de emisión o recepción del documento de cotización.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Monto total de la oferta (Suma de los ítems más impuestos si corresponde).
        /// Es el valor clave para la comparación económica entre proveedores.
        /// </summary>
        public decimal? TotalPrice { get; set; }

        /// <summary>
        /// Comentarios adicionales, términos de entrega o condiciones especiales 
        /// estipuladas por el proveedor.
        /// </summary>
        public string? Observations { get; set; }

        // =========================
        // RELACIONES 
        // =========================

        /// <summary>
        /// Lista de productos o servicios detallados con sus respectivos precios unitarios.
        /// </summary>
        public List<QuoteItem>? QuoteItems { get; set; }

        /// <summary>
        /// Identificador de la solicitud de compra (Purchase) a la cual responde esta cotización.
        /// </summary>
        public int? PurchaseId { get; set; }

        /// <summary>
        /// Propiedad de navegación hacia la compra de origen. 
        /// Permite agrupar múltiples cotizaciones de distintos proveedores bajo un mismo requerimiento.
        /// </summary>
        public Purchase? Purchase { get; set; } = null!;

        /// <summary>
        /// Identificador del proveedor que emite la cotización.
        /// </summary>
        public int? SupplierId { get; set; }

        /// <summary>
        /// Propiedad de navegación hacia el perfil del proveedor.
        /// </summary>
        public Supplier? Supplier { get; set; } = null!;

        /// <summary>
        /// Referencia a la Orden de Compra generada a partir de esta cotización.
        /// Solo existirá un registro aquí si la cotización fue la seleccionada/aceptada.
        /// </summary>
        public PurchaseOrder? PurchaseOrder { get; set; }
    }
}