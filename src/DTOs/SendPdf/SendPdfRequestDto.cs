using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.DTOs
{
    public record SendPdfRequestDto
    {
        public List<string> Emails { get; set; } = new();
        public PdfRequestDto PdfData { get; set; } = null!;
    }
}