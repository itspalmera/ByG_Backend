namespace ByG_Backend.src.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos optimizado para el listado y resumen de Órdenes de Compra.
    /// Consolida la información clave de diferentes entidades (Compra, Proveedor, Proyecto) 
    /// en una estructura plana ideal para tablas y búsquedas rápidas.
    /// </summary>
    /// <param name="Id">Identificador único de la Orden de Compra en la base de datos.</param>
    /// <param name="OrderNumber">Folio oficial de la OC (ej: OC-2026-005).</param>
    /// <param name="PurchaseNumber">Referencia al folio de la solicitud de compra original.</param>
    /// <param name="ProjectName">Nombre del proyecto o centro de costo destino (obra).</param>
    /// <param name="SupplierName">Razón social del proveedor adjudicado.</param>
    /// <param name="Date">Fecha de emisión del documento formateada para visualización.</param>
    /// <param name="TotalAmount">Valor total de la orden incluyendo impuestos y cargos adicionales.</param>
    /// <param name="Status">Estado actual del documento (ej: "Emitida", "Pendiente", "Anulada").</param>
    public record PurchaseOrderSummaryDto(
        int Id,
        string OrderNumber,
        string PurchaseNumber,
        string ProjectName,
        string SupplierName,
        string Date,
        decimal TotalAmount,
        string Status
    );
}