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
                Status = quote.Status,
                TotalPrice = quote.TotalPrice,
                Items = quote.Items?.Select(i => i.Name).ToArray() ?? Array.Empty<string>(),
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