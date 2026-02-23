using ByG_Backend.src.DTOs;
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
                CostCenter = po.CostCenter ?? po.Purchase?.ProjectName, // Si no hay C.Costo explícito, usamos el nombre del proyecto como fallback visual

                // --- Referencias ---
                PurchaseId = po.PurchaseId,
                PurchaseNumber = po.Purchase?.PurchaseNumber ?? "N/A",
                ProjectName = po.Purchase?.ProjectName ?? "N/A",
                
                // --- Datos del Proveedor (Extraídos de la relación Quote -> Supplier) ---
                Supplier = new SupplierInfoDto(
                    Rut: po.Quote?.Supplier?.Rut ?? "N/A",
                    BusinessName: po.Quote?.Supplier?.BusinessName ?? "Desconocido",
                    Email: po.Quote?.Supplier?.Email ?? "N/A",
                    Phone: po.Quote?.Supplier?.Phone,
                    Address: po.Quote?.Supplier?.Address,
                    City: po.Quote?.Supplier?.City,
                    ContactName: po.Quote?.Supplier?.ContactName
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
                    Name: i.Name,
                    Description: i.Description,
                    Unit: i.Unit,
                    Quantity: i.Quantity,
                    UnitPrice: i.UnitPrice ?? 0, // El precio ya debe venir de la cotización
                    TotalPrice: (i.UnitPrice ?? 0) * i.Quantity
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
                Status = "Emitida", // Estado inicial por defecto [cite: 379]
                
                // Relaciones
                PurchaseId = dto.PurchaseId,
                QuoteId = dto.QuoteId,

                // Datos Logísticos ingresados por el usuario al crear
                PaymentForm = dto.PaymentForm,
                PaymentTerms = dto.PaymentTerms,
                ExpectedDeliveryDate = dto.ExpectedDeliveryDate,
                ShippingAddress = dto.ShippingAddress,
                ShippingMethod = dto.ShippingMethod,
                Observations = dto.Observations,

                // Datos del Aprobador (si vienen del front o del token)
                ApproverName = dto.ApproverName,
                ApproverRut = dto.ApproverRut,
                ApproverRole = dto.ApproverRole,
                SignedAt = DateTime.UtcNow, // Se asume firmada al crearse/aprobarse

                // Valores por defecto para inicializar (serán sobrescritos por la lógica de negocio)
                Currency = "CLP",
                TaxRate = 19m // 19% Chile por defecto
            };
        }

        // 4. UPDATE DTO -> MODEL
        // Para editar datos logísticos o de forma de pago sin alterar los totales ni productos.
        public static void UpdateModel(this PurchaseOrder po, UpdatePurchaseOrderDto dto)
        {
            if (!string.IsNullOrEmpty(dto.PaymentForm)) po.PaymentForm = dto.PaymentForm;
            if (!string.IsNullOrEmpty(dto.PaymentTerms)) po.PaymentTerms = dto.PaymentTerms;
            if (!string.IsNullOrEmpty(dto.ShippingAddress)) po.ShippingAddress = dto.ShippingAddress;
            if (!string.IsNullOrEmpty(dto.ShippingMethod)) po.ShippingMethod = dto.ShippingMethod;
            if (!string.IsNullOrEmpty(dto.Observations)) po.Observations = dto.Observations;
            
            if (dto.ExpectedDeliveryDate.HasValue) po.ExpectedDeliveryDate = dto.ExpectedDeliveryDate;
        }
    }
}