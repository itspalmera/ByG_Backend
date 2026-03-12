using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.DTOs
{
    public class PdfItemData
    {
        public string Name { get; set; } = null!;
        public int Quantity { get; set; }
        public string Unit { get; set; } = null!;
    }
}