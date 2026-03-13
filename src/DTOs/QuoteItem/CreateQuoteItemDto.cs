using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos para la creación de un ítem dentro de una cotización.
    /// Define los valores económicos y cuantitativos que el proveedor ha ofertado por un producto específico.
    /// </summary>
    public class CreateQuoteItemDto
    {
        /// <summary>
        /// Nombre o descripción del material cotizado. 
        /// Generalmente coincide con el nombre del ítem en la solicitud de compra original.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Cantidad de unidades que se están cotizando.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Unidad de medida (ej: "Sacos", "Global", "Metro").
        /// </summary>
        public string Unit { get; set; } = null!;

        /// <summary>
        /// Precio unitario ofertado por el proveedor antes de impuestos y descuentos.
        /// </summary>
        public decimal UnitPrice { get; set; }
    }
}