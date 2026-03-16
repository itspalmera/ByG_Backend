using ByG_Backend.src.DTOs;
using ByG_Backend.src.Helpers;
using ByG_Backend.src.Models;

namespace ByG_Backend.src.Mappers
{
    /// <summary>
    /// Mapper estático para la entidad PurchaseOrder. 
    /// Proporciona métodos de extensión para transformar modelos de base de datos en DTOs 
    /// y viceversa, facilitando el manejo de resúmenes, vistas detalladas y actualizaciones.
    /// </summary>
    public static class PurchaseOrderMapper
    {
        /// <summary>
        /// Transforma una entidad PurchaseOrder en un DTO resumido para listados y tablas.
        /// </summary>
        /// <remarks>
        /// Incluye navegación segura hacia Purchase y Supplier para evitar excepciones de referencia nula
        /// si las relaciones no están cargadas.
        /// </remarks>
        /// <param name="po">Entidad de Orden de Compra.</param>
        /// <returns>DTo ligero con información clave para grids.</returns>
        public static PurchaseOrderSummaryDto ToSummaryDto(this PurchaseOrder po)
        {
            return new PurchaseOrderSummaryDto(
                po.Id,
                po.OrderNumber,
                po.Purchase?.PurchaseNumber ?? "Sin Referencia", 
                po.Purchase?.ProjectName ?? "Sin Proyecto",
                po.Quote?.Supplier?.BusinessName ?? "Proveedor Desconocido",
                po.Date.ToString("dd/MM/yyyy HH:mm:ss"),
                po.TotalAmount,
                po.Status
            );
        }

        /// <summary>
        /// Realiza un mapeo profundo de la Orden de Compra para vistas de detalle y reportes.
        /// </summary>
        /// <remarks>
        /// Aplana la jerarquía de datos, extrayendo información del Proveedor y los ítems 
        /// directamente desde la Cotización (Quote) asociada a la orden.
        /// </remarks>
        /// <param name="po">Entidad de Orden de Compra cargada con sus relaciones.</param>
        /// <returns>DTO detallado con toda la información logística, financiera y de aprobación.</returns>
        public static PurchaseOrderDetailDto ToDetailDto(this PurchaseOrder po)
        {
            return new PurchaseOrderDetailDto
            {
                // --- Identificación ---
                Id = po.Id,
                OrderNumber = po.OrderNumber,
                Status = po.Status,
                Date = po.Date.ToString("dd/MM/yyyy HH:mm:ss"),
                CostCenter = po.CostCenter ?? po.Purchase?.ProjectName, 

                // --- Referencias de Origen ---
                PurchaseId = po.PurchaseId,
                PurchaseNumber = po.Purchase?.PurchaseNumber ?? "N/A",
                ProjectName = po.Purchase?.ProjectName ?? "N/A",
                
                // --- Información del Proveedor ---
                Supplier = new SupplierInfoDto(
                    po.Quote?.Supplier?.Rut ?? "N/A",
                    po.Quote?.Supplier?.BusinessName ?? "N/A",
                    po.Quote?.Supplier?.Email ?? "N/A",
                    po.Quote?.Supplier?.Phone,
                    po.Quote?.Supplier?.Address,
                    po.Quote?.Supplier?.City,
                    po.Quote?.Supplier?.ContactName
                ),

                // --- Condiciones Logísticas ---
                PaymentForm = po.PaymentForm,
                PaymentTerms = po.PaymentTerms,
                Currency = po.Currency,
                ExpectedDeliveryDate = po.ExpectedDeliveryDate,
                DeliveryDeadline = po.DeliveryDeadline?.ToString("dd/MM/yyyy HH:mm:ss"),
                ShippingAddress = po.ShippingAddress,
                ShippingMethod = po.ShippingMethod,
                Observations = po.Observations,

                // --- Ítems Aprobados (desde la Cotización) ---
                Items = po.Quote?.QuoteItems?.Select(i => new PurchaseOrderItemDto(
                    i.Name,
                    i.Description,
                    i.Unit,
                    i.Quantity,
                    i.UnitPrice ?? 0, (i.UnitPrice ?? 0) * i.Quantity
                )).ToList() ?? new List<PurchaseOrderItemDto>(),

                // --- Resumen Financiero (Snapshot de BD) ---
                SubTotal = po.SubTotal,
                Discount = po.Discount,
                FreightCharge = po.FreightCharge,
                TaxExemptTotal = po.TaxExemptTotal,
                TaxRate = po.TaxRate,
                TaxAmount = po.TaxAmount,
                TotalAmount = po.TotalAmount,

                // --- Control de Firma ---
                ApproverName = po.ApproverName,
                ApproverRole = po.ApproverRole,
                SignedAt = po.SignedAt?.ToString("dd/MM/yyyy HH:mm:ss")
            };
        }

        /// <summary>
        /// Mapea un DTO de creación a una nueva entidad de modelo.
        /// </summary>
        /// <remarks>
        /// Establece el estado inicial "Esperando Aprobación" y fija la tasa de impuesto (IVA) al 19%.
        /// Nota: El cálculo de montos finales debe ser realizado por la lógica de negocio del controlador/servicio.
        /// </remarks>
        /// <param name="dto">Datos de creación enviados desde el cliente.</param>
        /// <param name="orderNumber">Folio de orden generado por el backend.</param>
        /// <returns>Entidad lista para persistencia inicial.</returns>
        public static PurchaseOrder ToModelFromCreate(this CreatePurchaseOrderDto dto, string orderNumber)
        {
            return new PurchaseOrder
            {
                OrderNumber = orderNumber, 
                Date = DateTime.UtcNow,
                Status = PurchaseOrderStatuses.WaitingApproval,
                
                PurchaseId = dto.PurchaseId,
                QuoteId = dto.QuoteId,

                CostCenter = dto.CostCenter,
                PaymentForm = dto.PaymentForm,
                PaymentTerms = dto.PaymentTerms,
                ExpectedDeliveryDate = dto.ExpectedDeliveryDate,
                DeliveryDeadline = dto.DeliveryDeadline,
                ShippingAddress = dto.ShippingAddress,
                ShippingMethod = dto.ShippingMethod,
                Observations = dto.Observations,
                Currency = dto.Currency,

                Discount = dto.Discount,
                FreightCharge = dto.FreightCharge,
                TaxRate = 19m // Valor por defecto para Chile
            };
        }

        /// <summary>
        /// Actualiza los campos logísticos y financieros base de una Orden de Compra existente.
        /// </summary>
        /// <param name="po">Entidad original a modificar.</param>
        /// <param name="dto">DTO con los nuevos valores (soporta actualizaciones parciales).</param>
        public static void UpdateModel(this PurchaseOrder po, UpdatePurchaseOrderDto dto)
        {
            if (dto.CostCenter != null) po.CostCenter = dto.CostCenter;
            if (dto.PaymentForm != null) po.PaymentForm = dto.PaymentForm;
            if (dto.PaymentTerms != null) po.PaymentTerms = dto.PaymentTerms;
            if (dto.ShippingAddress != null) po.ShippingAddress = dto.ShippingAddress;
            if (dto.ShippingMethod != null) po.ShippingMethod = dto.ShippingMethod;
            if (dto.Observations != null) po.Observations = dto.Observations;
            
            if (dto.ExpectedDeliveryDate.HasValue) po.ExpectedDeliveryDate = dto.ExpectedDeliveryDate;
            if (dto.DeliveryDeadline.HasValue) po.DeliveryDeadline = dto.DeliveryDeadline;
            
            if (dto.Discount.HasValue) po.Discount = dto.Discount.Value;
            if (dto.FreightCharge.HasValue) po.FreightCharge = dto.FreightCharge.Value;
        }
    }
}