using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ByG_Backend.src.DTOs;
using ByG_Backend.src.Models;

namespace ByG_Backend.src.Mappers
{
    /// <summary>
    /// Clase encargada de transformar la entidad <see cref="RequestQuote"/> en su representación de transferencia de datos <see cref="RequestQuoteDto"/>.
    /// Facilita la visualización del estado de las solicitudes enviadas a proveedores y su relación con las compras.
    /// </summary>
    public class RequestQuoteMapper
    {
        /// <summary>
        /// Convierte una instancia del modelo <see cref="RequestQuote"/> en un objeto <see cref="RequestQuoteDto"/>.
        /// </summary>
        /// <remarks>
        /// Realiza el formateo de fechas a cadenas legibles ("dd/MM/yyyy HH:mm:ss") y proyecta la lista de proveedores 
        /// asociados mediante la tabla intermedia <see cref="RequestQuoteSuppliers"/>.
        /// </remarks>
        /// <param name="requestQuote">Entidad de la solicitud de cotización recuperada de la base de datos.</param>
        /// <returns>Un DTO con la información de la solicitud y sus proveedores invitados.</returns>
        public static RequestQuoteDto RequestQuoteToRequestQuoteDto(RequestQuote requestQuote)
        {
            return new RequestQuoteDto
            {
                Id = requestQuote.Id,
                Number = requestQuote.Number,
                Status = requestQuote.Status,
                
                CreatedAt = requestQuote.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss"),
                SentAt = requestQuote.SentAt?.ToString("dd/MM/yyyy HH:mm:ss"),
                
                PurchaseId = requestQuote.PurchaseId,
                
                RequestQuoteSuppliers = requestQuote.RequestQuoteSuppliers.Select(s => new RequestQuoteSupplierDto
                {
                    SentAt = s.SentAt.ToString("dd/MM/yyyy HH:mm:ss"),
                    RequestQuoteId = s.RequestQuoteId,
                    SupplierId = s.SupplierId
                }).ToList()
            };
        }
    }
}