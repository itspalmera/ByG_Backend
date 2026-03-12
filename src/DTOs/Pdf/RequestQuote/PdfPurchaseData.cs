using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.DTOs
{
    /// <summary>
    /// Modelo de datos diseñado para la generación de reportes PDF de Requerimientos de Compra.
    /// Agrupa la información administrativa del origen de la solicitud y el desglose técnico de materiales.
    /// </summary>
    public class PdfPurchaseData
    {
        /// <summary>
        /// Identificador único de la compra en la base de datos.
        /// </summary>
        public int IdPurchase { get; set; }

        /// <summary>
        /// Folio interno de la solicitud (ej: SOL-2026-001).
        /// </summary>
        public string PurchaseNumber { get; set; } = null!;

        /// <summary>
        /// Nombre de la obra o proyecto de destino donde se utilizarán los materiales.
        /// </summary>
        public string ProjectName { get; set; } = null!;

        /// <summary>
        /// Nombre del responsable o departamento que emite el requerimiento desde terreno.
        /// </summary>
        public string Requester { get; set; } = null!;

        /// <summary>
        /// Lista de materiales o servicios solicitados, detallando cantidades y unidades, 
        /// pero sin información de costos.
        /// </summary>
        public List<PdfItemData> PurchaseItems { get; set; } = new();
    }
}