using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ByG_Backend.src.Models;

namespace ByG_Backend.src.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos utilizado para la solicitud de generación de reportes PDF de tipo RFQ.
    /// Encapsula las entidades de dominio necesarias para construir el documento de Solicitud de Cotización.
    /// </summary>
    public class GenerarPdfRequestDto
    {
        /// <summary>
        /// Entidad de compra que contiene el detalle de materiales, el proyecto de origen y el solicitante.
        /// Proporciona el contexto técnico de lo que se requiere cotizar.
        /// </summary>
        public Purchase Compra { get; set; } = null!;

        /// <summary>
        /// Entidad de solicitud de cotización que contiene el folio (Number) y los datos de gestión del RFQ.
        /// Proporciona el encabezado administrativo para el documento.
        /// </summary>
        public RequestQuote Solicitud { get; set; } = null!;
    }
}