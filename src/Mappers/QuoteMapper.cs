using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ByG_Backend.src.DTOs;
using ByG_Backend.src.Models;

namespace ByG_Backend.src.Mappers
{
    public class QuoteMapper
    {
        public static QuoteDto QuoteToQuoteDto(Quote quote) =>
            new()
            {
                Number = quote.Number,
                Status = quote.Status ?? "Pendiente",
                TotalPrice = quote.TotalPrice,
                Items = quote.QuoteItems?.Select(i => i.Name).ToArray() ?? Array.Empty<string>(),
                Date = quote.Date.ToString("dd/MM/yyyy")
            };

        public static UserDto UserToUserDto(User user) =>
            new()
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                IsActive = user.IsActive,
                Role = user.Role,
                Registered = user.Registered.ToString("dd/MM/yyyy"), 
                
            };

        // Método para actualizar un Quote existente desde un DTO
        public static void UpdateQuoteFromDto(Quote quote, UpdateQuoteDto dto)
        {
            if (!string.IsNullOrEmpty(dto.Status))
                quote.Status = dto.Status;
            
            if (dto.TotalPrice.HasValue)
                quote.TotalPrice = dto.TotalPrice;
            
            if (dto.Items != null)
            {
                quote.QuoteItems?.Clear();
                foreach (var itemName in dto.Items)
                {
                    quote.QuoteItems?.Add(new QuoteItem { 
                        Name = itemName,
                        Unit = "Unidad" 
                    });
                }
            }
        }

        // Método para crear un Quote desde un CreateQuoteDto
        public static Quote CreateQuoteFromDto(CreateQuoteDto dto)
        {

                DateTime fechaValida = DateTime.TryParse(dto.Date, out DateTime parsed) 
                ? parsed 
                : DateTime.Now;

            return new Quote
            {
                Number = dto.Number,
                Status = dto.Status ?? "Pendiente",
                TotalPrice = dto.TotalPrice,
                Date = fechaValida,
                SupplierId = dto.SupplierId, 
                PurchaseId = dto.PurchaseId,

                QuoteItems = dto.QuoteItems?.Select(itemDto => new QuoteItem 
                { 
                    Name = itemDto.Name,
                    Quantity = itemDto.Quantity,
                    UnitPrice = itemDto.UnitPrice,
                    Unit = itemDto.Unit ?? "Unidad" 
                }).ToList() ?? new List<QuoteItem>()
            };
        }
    }
}