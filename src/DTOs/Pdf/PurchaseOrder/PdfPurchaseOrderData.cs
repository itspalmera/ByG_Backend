/// <summary>
/// Modelo de datos optimizado para la generación de documentos PDF de Orden de Compra.
/// Contiene exclusivamente la información financiera y administrativa que debe visualizarse 
/// en la plantilla impresa del reporte.
/// </summary>
public class PdfPurchaseOrderData
{
    /// <summary>
    /// Número de folio oficial de la Orden de Compra (ej: OC-2026-001).
    /// </summary>
    public string OrderNumber { get; set; } = null!;

    /// <summary>
    /// Fecha de emisión que aparecerá en el encabezado del documento.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Centro de costo de ByG asociado para la imputación presupuestaria.
    /// </summary>
    public string? CostCenter { get; set; }

    /// <summary>
    /// Método de pago especificado en el documento (ej: Transferencia Bancaria).
    /// </summary>
    public string? PaymentForm { get; set; }

    /// <summary>
    /// Plazos o condiciones de pago (ej: 30 días contra factura).
    /// </summary>
    public string? PaymentTerms { get; set; }

    /// <summary>
    /// Moneda en la que se expresan los montos (ej: CLP, USD).
    /// </summary>
    public string Currency { get; set; } = "CLP";

    /// <summary>
    /// Monto del descuento aplicado a la orden para visualización en el pie del PDF.
    /// </summary>
    public decimal Discount { get; set; }

    /// <summary>
    /// Valor del cargo por flete o despacho incluido en la orden.
    /// </summary>
    public decimal FreightCharge { get; set; }

    /// <summary>
    /// Sumatoria de los valores netos de los ítems antes de impuestos.
    /// </summary>
    public decimal SubTotal { get; set; }

    /// <summary>
    /// Tasa impositiva aplicada (ej: 19.0 para IVA en Chile).
    /// </summary>
    public decimal TaxRate { get; set; }

    /// <summary>
    /// Monto total del impuesto calculado.
    /// </summary>
    public decimal TaxAmount { get; set; }

    /// <summary>
    /// Valor final total que el proveedor debe cobrar, incluyendo impuestos y recargos.
    /// </summary>
    public decimal TotalAmount { get; set; }
}