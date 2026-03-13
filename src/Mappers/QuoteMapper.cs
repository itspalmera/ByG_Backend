using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ByG_Backend.src.DTOs;
using ByG_Backend.src.Models;

namespace ByG_Backend.src.Mappers
{
    /// <summary>
    /// Clase encargada de realizar el mapeo de datos entre las entidades de Cotización (Quote) 
    /// y sus respectivos Objetos de Transferencia de Datos (DTOs).
    /// Facilita la creación, actualización y visualización de ofertas de proveedores.
    /// </summary>
    public class QuoteMapper
    {
        /// <summary>
        /// Convierte una entidad <see cref="Quote"/> en un <see cref="QuoteDto"/> para su exposición en la API.
        /// </summary>
        /// <remarks>
        /// Realiza un mapeo profundo incluyendo la lista de ítems detallados y resuelve 
        /// el nombre del proveedor si la relación está cargada.
        /// </remarks>
        /// <param name="quote">Entidad de cotización proveniente de la base de datos.</param>
        /// <returns>Un DTO con la información formateada para el frontend.</returns>
        public static QuoteDto QuoteToQuoteDto(Quote quote) =>
            new()
            {
                id = quote.Id.ToString(),
                Number = quote.Number,
                Status = quote.Status ?? "Pendiente",
                TotalPrice = quote.TotalPrice,
                Date = quote.Date.ToString("dd/MM/yyyy"),
                Observations = quote.Observations,
                SupplierName = quote.Supplier?.BusinessName ?? "Sin proveedor",
                Items = quote.QuoteItems?.Select(i => new QuoteItemDetailDto
                {
                    Name = i.Name,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Unit = i.Unit
                }).ToList() ?? new List<QuoteItemDetailDto>()
            };


        /// <summary>
        /// Actualiza las propiedades de una cotización existente utilizando los datos de un <see cref="UpdateQuoteDto"/>.
        /// </summary>
        /// <remarks>
        /// Si se proporcionan nuevos ítems, se limpia la colección actual y se regeneran como ítems genéricos.
        /// Este método es útil para actualizaciones rápidas de estado y precio total.
        /// </remarks>
        /// <param name="quote">La entidad de cotización a modificar.</param>
        /// <param name="dto">DTO con los nuevos valores.</param>
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

        /// <summary>
        /// Crea una nueva instancia del modelo <see cref="Quote"/> a partir de un <see cref="CreateQuoteDto"/>.
        /// </summary>
        /// <remarks>
        /// Incluye lógica de validación de fechas (fallback a la fecha actual) y asignación 
        /// de IDs de relación para Proveedor y Compra.
        /// </remarks>
        /// <param name="dto">Datos de entrada para la nueva cotización.</param>
        /// <returns>Una entidad <see cref="Quote"/> lista para ser insertada en el contexto de datos.</returns>
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
                Observations = dto.Observations,

                SupplierId = dto.SupplierId > 0 ? dto.SupplierId : null,
                PurchaseId = dto.PurchaseId > 0 ? dto.PurchaseId : null,

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