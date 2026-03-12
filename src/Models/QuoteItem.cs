using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.Models
{
    /// <summary>
    /// Representa una línea de detalle dentro de una cotización de proveedor.
    /// Almacena las condiciones económicas (precios) ofrecidas para un producto o servicio específico.
    /// </summary>
    public class QuoteItem
    {
        /// <summary>
        /// Identificador único del ítem de la cotización.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Número de línea correlativo dentro del documento para mantener el orden visual.
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// Nombre del producto o servicio cotizado.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Especificaciones técnicas o notas adicionales sobre el ítem ofrecido.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Unidad de medida (ej: "Global", "Unidad", "Kg").
        /// </summary>
        public string Unit { get; set; } = null!;

        /// <summary>
        /// Cantidad de unidades ofrecidas.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Valor monetario por unidad. Puede ser nulo si el proveedor aún no entrega el precio.
        /// </summary>
        public decimal? UnitPrice { get; set; }

        /// <summary>
        /// Cálculo del valor total de la línea (usualmente Quantity * UnitPrice).
        /// </summary>
        public decimal? TotalPrice { get; set; }

        // =========================
        // RELACIONES 
        // =========================

        /// <summary>
        /// Identificador de la cotización cabecera a la que pertenece este ítem.
        /// </summary>
        public int QuoteId { get; set; }

        /// <summary>
        /// Propiedad de navegación hacia la cotización (Quote) padre.
        /// </summary>
        public Quote Quote { get; set; } = null!;

        /// <summary>
        /// Identificador opcional del ítem de la solicitud de compra original.
        /// </summary>
        public int? PurchaseItemId { get; set; }

        /// <summary>
        /// Propiedad de navegación que vincula este precio con el requerimiento inicial.
        /// Permite saber qué producto específico de la solicitud de compra se está cotizando aquí.
        /// </summary>
        public PurchaseItem? PurchaseItem { get; set; }
    }
}