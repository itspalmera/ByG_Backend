using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.DTOs
{
    public record SendPdfDto
    {
        public string Email { get; set; } = null!;
        public byte[] PdfBytes { get; set; } = null!;
        public string FileName { get; set; } = null!;
    }
}