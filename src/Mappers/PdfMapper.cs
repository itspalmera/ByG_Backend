using ByG_Backend.src.DTOs;
using ByG_Backend.src.Models;

namespace ByG_Backend.src.Mappers
{
    public static class PdfMapper
    {
        public static (Purchase compra, RequestQuote solicitud) MapDtoToModels(PdfRequestDto dto)
        {
            // Mapeamos la Compra y sus Items
            var compra = new Purchase
            {
                PurchaseNumber = dto.Compra.PurchaseNumber,
                ProjectName = dto.Compra.ProjectName,
                Requester = dto.Compra.Requester,
                PurchaseItems = dto.Compra.PurchaseItems.Select(i => new PurchaseItem
                {
                    Name = i.Name,
                    Quantity = i.Quantity,
                    Unit = i.Unit
                }).ToList()
            };

            // Mapeamos la Solicitud
            var solicitud = new RequestQuote
            {
                Number = dto.Solicitud.Number,
                Status = dto.Solicitud.Status
            };

            return (compra, solicitud);
        }
    }
}