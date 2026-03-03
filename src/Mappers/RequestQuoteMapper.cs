using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ByG_Backend.src.DTOs;
using ByG_Backend.src.Models;

namespace ByG_Backend.src.Mappers
{
    public class RequestQuoteMapper
    {
        public static RequestQuoteDto RequestQuoteToRequestQuoteDto(RequestQuote requestQuote)
        {
            return new RequestQuoteDto
            {
                Id = requestQuote.Id,
                Number = requestQuote.Number,
                Status = requestQuote.Status,
                CreatedAt = requestQuote.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss"),
                SentAt = requestQuote.SentAt?.ToString("dd/MM/yyyy HH:mm:ss"),
                PurchaseId = requestQuote.PurchaseId,
                RequestQuoteSuppliers = requestQuote.RequestQuoteSuppliers.Select(s => new RequestQuoteSupplierDto
                {
                    SentAt = s.SentAt.ToString("dd/MM/yyyy HH:mm:ss"),
                    RequestQuoteId = s.RequestQuoteId,
                    SupplierId = s.SupplierId
                }).ToList()
            };
        }
    }
}