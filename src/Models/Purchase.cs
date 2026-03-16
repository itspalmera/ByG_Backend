using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ByG_Backend.src.Models
{
    /// <summary>
    /// Representa la entidad principal de un requerimiento de compra en el sistema.
    /// Actúa como el contenedor raíz que agrupa ítems, cotizaciones y documentos 
    /// finales para un proyecto u obra específica.
    /// </summary>
    public class Purchase
    {
        /// <summary>
        /// Identificador único autoincremental de la compra.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Folio interno que identifica el requerimiento (ej: SOL-2026-005).
        /// Es la referencia principal para los usuarios de ByG.
        /// </summary>
        public string PurchaseNumber { get; set; } = null!;

        /// <summary>
        /// Nombre de la obra, proyecto o faena a la cual se destinarán los materiales.
        /// </summary>
        public string ProjectName { get; set; } = null!;

        /// <summary>
        /// Estado actual del flujo de la compra (ej: "Recibida", "Cotizando", "Finalizada").
        /// Controla la lógica de negocio y permisos sobre el registro.
        /// </summary>
        public string Status { get; set; } = null!;

        /// <summary>
        /// Fecha y hora en la que se generó la solicitud original desde terreno.
        /// </summary>
        public DateTime RequestDate { get; set; } 

        /// <summary>
        /// Fecha de la última modificación significativa (cambio de estado, adición de cotizaciones, etc.).
        /// Se inicializa por defecto con la fecha UTC actual.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Identificación de la persona o departamento que solicita los materiales.
        /// </summary>
        public string Requester { get; set; } = null!;

        /// <summary>
        /// Notas adicionales o contexto relevante para el área de adquisiciones.
        /// </summary>
        public string? Observations { get; set; }

        // =========================
        // RELACIONES 
        // =========================

        /// <summary>
        /// Colección de productos o servicios individuales que componen el requerimiento.
        /// Relación de 1 a Muchos (Padre de PurchaseItem).
        /// </summary>
        public List<PurchaseItem> PurchaseItems { get; set; } = new(); 

        /// <summary>
        /// Conjunto de ofertas económicas recibidas de distintos proveedores para esta compra.
        /// Relación de 1 a Muchos (Padre de Quote).
        /// </summary>
        public List<Quote> Quotes { get; set; } = new();

        /// <summary>
        /// Referencia a la solicitud formal de cotización (RFQ) despachada a proveedores.
        /// Relación 1 a 1 (Principal).
        /// </summary>
        public RequestQuote? RequestQuote { get; set; }

        /// <summary>
        /// Referencia a la Orden de Compra legal que cierra el ciclo de adquisición.
        /// Relación 1 a 1 (Principal).
        /// </summary>
        public PurchaseOrder? PurchaseOrder { get; set; }
    }
}