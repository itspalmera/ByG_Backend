using ByG_Backend.src.Helpers;
using ByG_Backend.src.DTOs;
using ByG_Backend.src.Models;

namespace ByG_Backend.src.Mappers
{
    public static class PurchaseMapper
    {
        // 1. Modelo -> DTO (Para ver detalles de la Compra)
        public static PurchaseDetailDto ToDetailDto(this Purchase purchase)
        {
            return new PurchaseDetailDto(
                purchase.Id,
                purchase.PurchaseNumber,
                purchase.ProjectName,
                purchase.Status,
                purchase.RequestDate,
                purchase.UpdatedAt,
                purchase.Requester,
                purchase.Observations,
                
                // Mapeo de la lista de productos (hijos)
                PurchaseItems: purchase.PurchaseItems?.Select(item => new PurchaseItemDto(
                    item.Id,
                    item.Name,
                    item.BrandModel,
                    item.Description,
                    item.Unit,
                    item.Size,
                    item.Quantity
                )).ToList() ?? new List<PurchaseItemDto>(),

                // Banderas dinámicas para la UI del Frontend evaluando si las relaciones existen
                HasRequestQuote: purchase.RequestQuote != null,
                HasPurchaseOrder: purchase.PurchaseOrder != null
            );
        }

        // 2. DTO -> Modelo (Para Crear la Compra desde el Sistema Externo)
        public static Purchase ToModelFromCreate(this PurchaseCreateDto dto)
        {
            return new Purchase
            {
                PurchaseNumber = dto.PurchaseNumber,
                ProjectName = dto.ProjectName,
                Requester = dto.Requester,
                Observations = dto.Observations,
                RequestDate = DateTime.UtcNow,
                Status = PurchaseStatuses.Received, // Estado inicial al entrar al sistema
                
                // Generamos los hijos (PurchaseItems) inmediatamente junto con la compra
                PurchaseItems = dto.Items.Select(item => new PurchaseItem
                {
                    Name = item.Name,
                    BrandModel = item.BrandModel,
                    Description = item.Description,
                    Unit = item.Unit,
                    Size = item.Size,
                    Quantity = item.Quantity
                }).ToList()
            };
        }

        // 3. Mutar Modelo (Para Editar cabecera)
        public static void UpdateModel(this Purchase purchase, PurchaseUpdateDto dto)
        {
            purchase.ProjectName = dto.ProjectName;
            purchase.Requester = dto.Requester;
            purchase.Observations = dto.Observations;
            purchase.UpdatedAt = DateTime.UtcNow;
        }
    }
}