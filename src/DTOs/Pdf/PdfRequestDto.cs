using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.DTOs
{
    public class PdfRequestDto
    {
        public PdfPurchaseData Compra { get; set; } = null!;
        public PdfRequestData Solicitud { get; set; } = null!;
    }
}