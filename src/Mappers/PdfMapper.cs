using System;
using System.Collections.Generic;
using System.Linq;
using ByG_Backend.src.DTOs;
using ByG_Backend.src.Models;

namespace ByG_Backend.src.Mappers
{
    /// <summary>
    /// Clase estática encargada de transformar objetos de transferencia de datos (DTOs) 
    /// en entidades de modelo específicamente preparadas para la generación de documentos PDF.
    /// Facilita la desacoplación entre la entrada de la API y los servicios de impresión.
    /// </summary>
    public static class PdfMapper
    {
        /// <summary>
        /// Mapea un <see cref="PdfRequestDto"/> a una tupla que contiene la Compra y la Solicitud de Cotización.
        /// Este mapeo se utiliza principalmente para generar documentos de Solicitud (RFQ).
        /// </summary>
        /// <param name="dto">DTO con la información estructurada de la compra y la solicitud.</param>
        /// <returns>Una tupla con las entidades <see cref="Purchase"/> y <see cref="RequestQuote"/> pobladas.</returns>
        public static (Purchase compra, RequestQuote solicitud) MapDtoToModels(PdfRequestDto dto)
        {
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

            var solicitud = new RequestQuote
            {
                Id = dto.Solicitud.Id,
                Number = dto.Solicitud.Number,
                Status = dto.Solicitud.Status
            };

            return (compra, solicitud);
        }

        /// <summary>
        /// Mapea un <see cref="PurchaseOrderPdfDto"/> a una tupla que contiene la Orden, el Proveedor y los ítems.
        /// Este mapeo es esencial para la generación del documento final de Orden de Compra (OC).
        /// </summary>
        /// <param name="dto">DTO con los datos financieros de la orden, datos del proveedor e ítems cotizados.</param>
        /// <returns>Una tupla con la <see cref="PurchaseOrder"/>, el <see cref="Supplier"/> y una lista de <see cref="QuoteItem"/>.</returns>
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