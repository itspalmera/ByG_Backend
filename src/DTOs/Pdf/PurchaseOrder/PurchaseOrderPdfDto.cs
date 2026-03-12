using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.DTOs
{
    public class PurchaseOrderPdfDto
    {
         public PdfPurchaseOrderData Order { get; set; } = null!;
        public PdfSupplierData Supplier { get; set; } = null!;
        public List<PdfQuoteItemData> Items { get; set; } = new();
    }
}