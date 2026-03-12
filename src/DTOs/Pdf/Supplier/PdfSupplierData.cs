using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.DTOs
{
    public class PdfSupplierData
    {
        public string Name { get; set; } = null!;
    public string? Address { get; set; }
    public string? Rut { get; set; }
    public string? Phone { get; set; }
    public string? Contact { get; set; }
    }
}