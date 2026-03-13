using System;

namespace ByG_Backend.src.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos optimizado para listados de requerimientos de compra.
    /// Proporciona una vista ligera y de alto nivel para el seguimiento masivo de solicitudes.
    /// </summary>
    /// <param name="Id">Identificador único interno en la base de datos.</param>
    /// <param name="PurchaseNumber">Folio administrativo único del requerimiento (ej: SOL-001).</param>
    /// <param name="ProjectName">Nombre del proyecto o faena donde se destinarán los recursos.</param>
    /// <param name="Status">Estado actual del flujo (ej: "Solicitud enviada", "Orden emitida").</param>
    /// <param name="RequestDate">Fecha y hora en que se generó el requerimiento.</param>
    /// <param name="Requester">Nombre de la persona o departamento responsable del pedido.</param>
    /// <param name="ItemsCount">Cantidad total de líneas de materiales o servicios solicitados.</param>
    public record PurchaseSummaryDto(
        int Id,
        string PurchaseNumber,
        string ProjectName,
        string Status,
        DateTime RequestDate,
        string Requester,
        int ItemsCount
    );
}