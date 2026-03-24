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
    public static class RequestQuoteMapper
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
                
                RequestQuoteSuppliers = requestQuote.RequestQuoteSuppliers.Select(s => s.ToDto()).ToList()
            };
        }


        public static RequestQuoteSupplierDto ToDto(this RequestQuoteSupplier model)
        {
            return new RequestQuoteSupplierDto
            {
                SupplierId = model.SupplierId,
                RequestQuoteId = model.RequestQuoteId,
                
                // Si SentAt es "0001", lo mandamos vacío para no mostrar "01-01-0001"
                SentAt = model.SentAt.Year == 1 ? string.Empty : model.SentAt.ToString("dd/MM/yyyy HH:mm:ss"),
                
                // 👇 AHORA SÍ PASAMOS TODA LA INFO
                SupplierName = model.Supplier?.BusinessName ?? "Proveedor Desconocido",
                SupplierRut = model.Supplier?.Rut ?? "Sin RUT",
                SupplierEmail = model.Supplier?.Email ?? "Sin correo"
            };
        }
    }
}