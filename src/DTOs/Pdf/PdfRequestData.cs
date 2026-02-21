using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.DTOs
{
    public class PdfRequestData
    {
        public string Number { get; set; } = null!;
        public string Status { get; set; } = null!;
    }
}