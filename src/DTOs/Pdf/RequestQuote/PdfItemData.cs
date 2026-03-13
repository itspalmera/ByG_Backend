using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.DTOs
{
    /// <summary>
    /// Modelo de datos simplificado para la representación de ítems en documentos PDF.
    /// Se utiliza principalmente en reportes donde solo se requiere informar la cantidad 
    /// y descripción de materiales, omitiendo valores monetarios.
    /// </summary>
    public class PdfItemData
    {
        /// <summary>
        /// Nombre o descripción técnica del material solicitado.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Cantidad física requerida.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Unidad de medida del material (ej: "Sacos", "Global", "Tiras").
        /// </summary>
        public string Unit { get; set; } = null!;
    }
}