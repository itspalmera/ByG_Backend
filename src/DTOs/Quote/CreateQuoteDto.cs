using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos para la creación de una nueva Cotización.
    /// Valida la consistencia de la oferta del proveedor antes de su persistencia en el sistema.
    /// </summary>
    public record CreateQuoteDto
    {
        /// <summary>
        /// Número identificador de la cotización proporcionado por el proveedor.
        /// </summary>
        [Required(ErrorMessage = "El número de cotización es obligatorio.")]
        public required string Number { get; set; }

        /// <summary>
        /// Estado inicial de la cotización. 
        /// Restringido mediante validación de negocio a: Pendiente, Aprobada o Rechazada.
        /// </summary>
        [Required]
        [RegularExpression("^(Pendiente|Aprobada|Rechazada)$", 
            ErrorMessage = "El estado debe ser 'Pendiente', 'Aprobada' o 'Rechazada'.")]
        public required string Status { get; set; } 

        /// <summary>
        /// Fecha de emisión del documento. 
        /// Debe cumplir con el formato estándar YYYY-MM-DD para su correcto procesamiento.
        /// </summary>
        [Required]
        [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "La fecha debe tener el formato AAAA-MM-DD.")]
        public required string Date { get; set; }

        /// <summary>
        /// Monto total acumulado de la oferta. 
        /// Nulable para permitir el cálculo automático en el servidor si es necesario.
        /// </summary>
        public decimal? TotalPrice { get; set; }

        /// <summary>
        /// Comentarios adicionales o términos específicos de la oferta del proveedor.
        /// </summary>
        [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder los 500 caracteres.")]
        public string? Observations { get; set; }

        /// <summary>
        /// Listado de productos y precios unitarios que componen la oferta.
        /// </summary>
        [Required (ErrorMessage = "La lista de items no puede estar vacía.")]
        public List<CreateQuoteItemDto> QuoteItems { get; set; } = new();

        /// <summary>
        /// Identificador del proveedor que emite la cotización.
        /// </summary>
        public int? SupplierId { get; set; } 
        
        /// <summary>
        /// Identificador del requerimiento de compra asociado a esta cotización.
        /// </summary>
        public int? PurchaseId { get; set; }
    }
}