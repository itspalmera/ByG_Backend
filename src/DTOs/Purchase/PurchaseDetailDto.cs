using System.Collections.Generic;

namespace ByG_Backend.src.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos para la vista detallada de un requerimiento de compra.
    /// Consolida la información del encabezado, el desglose de ítems y el estado de avance 
    /// de los procesos posteriores (cotizaciones y órdenes de compra).
    /// </summary>
    /// <param name="Id">Identificador único en la base de datos.</param>
    /// <param name="PurchaseNumber">Folio administrativo del requerimiento.</param>
    /// <param name="ProjectName">Obra o proyecto de destino.</param>
    /// <param name="Status">Estado actual del flujo de trabajo.</param>
    /// <param name="RequestDate">Fecha de creación formateada para el cliente.</param>
    /// <param name="UpdatedAt">Fecha de la última modificación del registro.</param>
    /// <param name="Requester">Usuario o área que originó la solicitud.</param>
    /// <param name="Observations">Notas aclaratorias adicionales.</param>
    /// <param name="PurchaseItems">Colección detallada de materiales o servicios solicitados.</param>
    /// <param name="RequestQuote">Información de la solicitud de cotización vinculada, si existe.</param>
    /// <param name="HasPurchaseOrder">Bandera lógica para control de UI: indica si el proceso ya culminó en una OC.</param>
    /// <param name="PurchaseOrderId">Referencia directa a la Orden de Compra final para navegación rápida.</param>
    public record PurchaseDetailDto(
        int Id,
        string PurchaseNumber,
        string ProjectName,
        string Status,
        string RequestDate,
        string UpdatedAt,
        string Requester,
        string? Observations,
        
        List<PurchaseItemDto> PurchaseItems,

        RequestQuoteDto? RequestQuote,
        bool HasPurchaseOrder,
        int? PurchaseOrderId
    );
}