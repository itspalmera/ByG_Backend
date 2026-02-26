using ByG_Backend.src.DTOs;
using ByG_Backend.src.Helpers;
using ByG_Backend.src.Models;

namespace ByG_Backend.src.Mappers
{
    public static class PurchaseOrderMapper
    {
        // 1. TO SUMMARY DTO (Para el Grid/Tabla en NextJS)
        // Convierte la entidad compleja en una fila ligera para listar.
        public static PurchaseOrderSummaryDto ToSummaryDto(this PurchaseOrder po)
        {
            return new PurchaseOrderSummaryDto(
                po.Id,
                po.OrderNumber,
                po.Purchase?.PurchaseNumber ?? "Sin Referencia", // Folio Solicitud
                po.Purchase?.ProjectName ?? "Sin Proyecto",
                po.Quote?.Supplier?.BusinessName ?? "Proveedor Desconocido",
                po.Date,
                po.TotalAmount,
                po.Status
            );
        }

        // 2. TO DETAIL DTO (Para Vista Detallada y Generación de PDF)
        // Aplana la estructura jerárquica para facilitar el consumo.
        public static PurchaseOrderDetailDto ToDetailDto(this PurchaseOrder po)
        {
            return new PurchaseOrderDetailDto
            {
                // --- Encabezado ---
                Id = po.Id,
                OrderNumber = po.OrderNumber,
                Status = po.Status,
                Date = po.Date,
                CostCenter = po.CostCenter ?? "N/A", // Si no hay C.Costo explícito, usamos el nombre del proyecto como fallback visual

                // --- Referencias ---
                PurchaseId = po.PurchaseId,
                PurchaseNumber = po.Purchase?.PurchaseNumber ?? "N/A",
                ProjectName = po.Purchase?.ProjectName ?? "N/A",
                
                // --- Datos del Proveedor (Extraídos de la relación Quote -> Supplier) ---
                Supplier = new SupplierInfoDto(
                    po.Quote?.Supplier?.Rut ?? "N/A",
                    po.Quote?.Supplier?.BusinessName ?? "N/A",
                    po.Quote?.Supplier?.Email ?? "N/A",
                    po.Quote?.Supplier?.Phone,
                    po.Quote?.Supplier?.Address,
                    po.Quote?.Supplier?.City,
                    po.Quote?.Supplier?.ContactName
                ),

                // --- Logística y Pago ---
                PaymentForm = po.PaymentForm,
                PaymentTerms = po.PaymentTerms,
                Currency = po.Currency,
                ExpectedDeliveryDate = po.ExpectedDeliveryDate,
                DeliveryDeadline = po.DeliveryDeadline,
                ShippingAddress = po.ShippingAddress,
                ShippingMethod = po.ShippingMethod,
                Observations = po.Observations,

                // --- Items (Extraídos de Quote -> QuoteItems) ---
                // Aquí tomamos los items de la cotización ganadora.
                Items = po.Quote?.QuoteItems?.Select(i => new PurchaseOrderItemDto(
                    i.Name,
                    i.Description,
                    i.Unit,
                    i.Quantity,
                    i.UnitPrice ?? 0, (i.UnitPrice ?? 0) * i.Quantity
                )).ToList() ?? new List<PurchaseOrderItemDto>(),

                // --- Totales (Snapshot guardado en BD) ---
                SubTotal = po.SubTotal,
                Discount = po.Discount,
                FreightCharge = po.FreightCharge,
                TaxExemptTotal = po.TaxExemptTotal,
                TaxRate = po.TaxRate,
                TaxAmount = po.TaxAmount,
                TotalAmount = po.TotalAmount,

                // --- Aprobación ---
                ApproverName = po.ApproverName,
                ApproverRole = po.ApproverRole,
                SignedAt = po.SignedAt
            };
        }

        // 3. CREATE DTO -> MODEL
        // Mapea los datos iniciales. NOTA: Los cálculos de montos NO se hacen aquí,
        // se deben hacer en el Controller/Service para garantizar integridad.
        public static PurchaseOrder ToModelFromCreate(this CreatePurchaseOrderDto dto, string orderNumber)
        {
            return new PurchaseOrder
            {
                OrderNumber = orderNumber, // Generado por el backend (ej: OC-2026-001)
                Date = DateTime.UtcNow,
                // ESTADO INICIAL REQUERIDO
                Status = PurchaseOrderStatuses.WaitingApproval,
                
                // Relaciones
                PurchaseId = dto.PurchaseId,
                QuoteId = dto.QuoteId,

                // Datos Logísticos ingresados por el usuario al crear/formalizar la OC

                CostCenter = dto.CostCenter,
                PaymentForm = dto.PaymentForm,
                PaymentTerms = dto.PaymentTerms,
                ExpectedDeliveryDate = dto.ExpectedDeliveryDate,
                DeliveryDeadline = dto.DeliveryDeadline,
                ShippingAddress = dto.ShippingAddress,
                ShippingMethod = dto.ShippingMethod,
                Observations = dto.Observations,
                Currency = dto.Currency,

                // Montos base (cálculo final en controller)
                Discount = dto.Discount,
                FreightCharge = dto.FreightCharge,
                TaxRate = 19m, // 19% Chile por defecto

                // Datos del Aprobador (si vienen del front o del token)
                ApproverName = dto.ApproverName,
                ApproverRut = dto.ApproverRut,
                ApproverRole = dto.ApproverRole,
                SignedAt = null // Se llena cuando se aprueba la OC


            };
        }

        // 4. UPDATE DTO -> MODEL
        // Para editar datos logísticos o de forma de pago sin alterar los totales ni productos.
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
                
                // Los montos financieros se actualizan aquí, pero el recálculo total lo hace el Controller
                if (dto.Discount.HasValue) po.Discount = dto.Discount.Value;
                if (dto.FreightCharge.HasValue) po.FreightCharge = dto.FreightCharge.Value;
        }
    }
}