using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ByG_Backend.src.Models;
using ByG_Backend.src.DTOs;

namespace ByG_Backend.src.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos que representa una Solicitud de Cotización (RFQ) completa.
    /// Proporciona una visión integral del proceso de licitación, vinculando el requerimiento 
    /// original con los proveedores participantes.
    /// </summary>
    public record RequestQuoteDto
    {
        /// <summary>
        /// Identificador único de la solicitud en la base de datos.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Número de folio administrativo (ej: RFQ-2026-001).
        /// </summary>
        public string Number { get; set; } = null!;

        /// <summary>
        /// Estado actual del proceso (ej: "Pendiente", "Enviada", "Finalizada").
        /// </summary>
        public string Status { get; set; } = null!;

        /// <summary>
        /// Fecha de creación del registro en el sistema.
        /// </summary>
        public string CreatedAt { get; set; } = null!;

        /// <summary>
        /// Fecha y hora exacta en la que se despacharon los correos a los proveedores.
        /// </summary>
        public string? SentAt { get; set; }

        /// <summary>
        /// ID del requerimiento de compra de origen.
        /// </summary>
        public int PurchaseId { get; set; }

        /// <summary>
        /// Resumen o descripción de la compra para dar contexto rápido en la lista de solicitudes.
        /// </summary>
        public string PurchaseDescription { get; set; } = string.Empty;

        /// <summary>
        /// Listado de proveedores asociados a esta solicitud, incluyendo el estado de su participación.
        /// </summary>
        public List<RequestQuoteSupplierDto> RequestQuoteSuppliers { get; set; } = new();
    }
}