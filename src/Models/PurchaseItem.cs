using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ByG_Backend.src.Models
{
    /// <summary>
    /// Representa un producto o material específico solicitado dentro de un requerimiento de compra.
    /// Define las características técnicas y cantidades necesarias antes de ser valorizado por un proveedor.
    /// </summary>
    public class PurchaseItem
    {
        /// <summary>
        /// Identificador único autoincremental del ítem de compra.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre del material o producto solicitado (ej: "Cemento", "Guantes de seguridad").
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Especificación de Marca o Modelo preferido o referencial (ej: "3M", "Caterpillar").
        /// </summary>
        public string? BrandModel { get; set; }

        /// <summary>
        /// Detalle o descripción extendida del producto para mayor claridad del proveedor.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Unidad de medida del producto (ej: "UN", "Saco", "KG", "MT").
        /// </summary>
        public string Unit { get; set; } = null!;

        /// <summary>
        /// Talla, medida o dimensión específica del ítem (ej: "XL", "1/2 pulgada", "42").
        /// </summary>
        public string? Size { get; set; }

        /// <summary>
        /// Cantidad total requerida de este producto.
        /// </summary>
        public int Quantity { get; set; }

        // =========================
        // RELACIONES 
        // =========================

        /// <summary>
        /// Identificador de la compra cabecera a la que pertenece este ítem.
        /// </summary>
        public int PurchaseId { get; set; }

        /// <summary>
        /// Propiedad de navegación hacia la solicitud de compra (Purchase) padre.
        /// </summary>
        public Purchase Purchase { get; set; } = null!;

        /// <summary>
        /// Lista de cotizaciones recibidas para este ítem específico.
        /// Permite comparar los diferentes precios ofrecidos por distintos proveedores para un mismo producto.
        /// </summary>
        public List<QuoteItem> QuoteItems { get; set; } = new();
    }
}