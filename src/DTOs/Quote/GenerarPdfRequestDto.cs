using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ByG_Backend.src.Models;

namespace ByG_Backend.src.DTOs
{
    public class GenerarPdfRequestDto
    {
        public Purchase Compra { get; set; }
        public RequestQuote Solicitud { get; set;}
    }
}