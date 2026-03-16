using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos para la visualización detallada de un ítem cotizado.
    /// Se utiliza en las vistas de consulta y comparación de ofertas dentro del sistema.
    /// </summary>
    public class QuoteItemDetailDto
    {
        /// <summary>
        /// Nombre o descripción del material o servicio.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Cantidad de unidades registradas en la oferta.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Precio unitario ofertado. Es opcional para permitir la visualización 
        /// de ítems en estados donde el precio aún no se ha definido.
        /// </summary>
        public decimal? UnitPrice { get; set; }

        /// <summary>
        /// Unidad de medida del ítem (ej: "Global", "M2", "Kg").
        /// </summary>
        public string Unit { get; set; } = null!;
    }
}