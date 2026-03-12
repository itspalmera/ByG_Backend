using ByG_Backend.src.DTOs;
using ByG_Backend.src.Models;

namespace ByG_Backend.src.Mappers
{
    public static class PdfMapper
    {
        public static (Purchase compra, RequestQuote solicitud) MapDtoToModels(PdfRequestDto dto)
        {
            // Mapeamos la Compra y sus Items
            var compra = new Purchase
            {
                Id = dto.Compra.IdPurchase,
                PurchaseNumber = dto.Compra.PurchaseNumber,
                ProjectName = dto.Compra.ProjectName,
                Requester = dto.Compra.Requester,
                PurchaseItems = dto.Compra.PurchaseItems.Select(i => new PurchaseItem
                {
                    Name = i.Name,
                    Quantity = i.Quantity,
                    Unit = i.Unit
                }).ToList()
            };

            // Mapeamos la Solicitud
            var solicitud = new RequestQuote
            {
                Id = dto.Solicitud.Id,
                Number = dto.Solicitud.Number,
                Status = dto.Solicitud.Status
            };

            return (compra, solicitud);
        }





        public static (PurchaseOrder order, Supplier supplier, List<QuoteItem> items) MapDtoToModels(PurchaseOrderPdfDto dto)
    {
        var order = new PurchaseOrder
        {
            OrderNumber = dto.Order.OrderNumber,
            Date = dto.Order.Date,
            CostCenter = dto.Order.CostCenter,
            PaymentForm = dto.Order.PaymentForm,
            PaymentTerms = dto.Order.PaymentTerms,
            Currency = dto.Order.Currency,
            Discount = dto.Order.Discount,
            FreightCharge = dto.Order.FreightCharge,
            SubTotal = dto.Order.SubTotal,
            TaxRate = dto.Order.TaxRate,
            TaxAmount = dto.Order.TaxAmount,
            TotalAmount = dto.Order.TotalAmount
        };

        var supplier = new Supplier
        {
            BusinessName = dto.Supplier.Name,
            Address = dto.Supplier.Address,
            Rut = dto.Supplier.Rut,
            Phone = dto.Supplier.Phone,
            ContactName = dto.Supplier.Contact
        };

        var items = dto.Items.Select(i => new QuoteItem
        {
            Description = i.Description,
            Quantity = i.Quantity,
            Unit = i.Unit,
            UnitPrice = i.UnitPrice
        }).ToList();

        return (order, supplier, items);
    }
    }
}