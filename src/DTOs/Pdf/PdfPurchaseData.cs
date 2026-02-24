using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.DTOs
{
    public class PdfPurchaseData
    {
        public int IdPurchase { get; set; }
        public string PurchaseNumber { get; set; } = null!;
        public string ProjectName { get; set; } = null!;
        public string Requester { get; set; } = null!;
        public List<PdfItemData> PurchaseItems { get; set; } = new();
    }
}