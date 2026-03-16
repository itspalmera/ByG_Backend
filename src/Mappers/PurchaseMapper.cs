using System;
using System.Collections.Generic;
using System.Linq;
using ByG_Backend.src.Helpers;
using ByG_Backend.src.DTOs;
using ByG_Backend.src.Models;

namespace ByG_Backend.src.Mappers
{
    /// <summary>
    /// Clase estática que contiene métodos de extensión para el mapeo entre la entidad <see cref="Purchase"/> 
    /// y sus diferentes representaciones de transferencia de datos (DTOs).
    /// Centraliza la lógica de transformación para mantener los controladores limpios y legibles.
    /// </summary>
    public static class PurchaseMapper
    {
        /// <summary>
        /// Convierte una entidad de modelo <see cref="Purchase"/> en un <see cref="PurchaseDetailDto"/>.
        /// </summary>
        /// <remarks>
        /// Este método incluye el mapeo de colecciones anidadas (ítems de compra) y la lógica para verificar 
        /// si existe una solicitud de cotización o una orden de compra vinculada.
        /// </remarks>
        /// <param name="purchase">La entidad de compra recuperada de la base de datos.</param>
        /// <returns>Un DTO detallado listo para ser consumido por la interfaz de usuario.</returns>
        public static PurchaseDetailDto ToDetailDto(this Purchase purchase)
        {
            return new PurchaseDetailDto(
                purchase.Id,
                purchase.PurchaseNumber,
                purchase.ProjectName,
                purchase.Status,
                purchase.RequestDate.ToString("dd/MM/yyyy HH:mm:ss"),
                purchase.UpdatedAt.ToString("dd/MM/yyyy HH:mm:ss"),
                purchase.Requester,
                purchase.Observations,
                
                purchase.PurchaseItems?.Select(item => new PurchaseItemDto(
                    item.Id, item.Name, item.BrandModel, item.Description, 
                    item.Unit, item.Size, item.Quantity
                )).ToList() ?? new List<PurchaseItemDto>(),

                RequestQuote: purchase.RequestQuote != null 
                    ? RequestQuoteMapper.RequestQuoteToRequestQuoteDto(purchase.RequestQuote) 
                    : null,

                HasPurchaseOrder: purchase.PurchaseOrder != null,
                PurchaseOrderId: purchase.PurchaseOrder?.Id
            );
        }

        /// <summary>
        /// Transforma un DTO de creación en una nueva instancia del modelo <see cref="Purchase"/>.
        /// </summary>
        /// <remarks>
        /// Al crear la compra, se asigna automáticamente la fecha actual en formato UTC y se establece 
        /// el estado inicial definido en <see cref="PurchaseStatuses.Received"/>.
        /// También se realiza el mapeo inmediato de los ítems hijos para una inserción atómica.
        /// </remarks>
        /// <param name="dto">Datos de entrada enviados por el sistema externo o frontend.</param>
        /// <returns>Una entidad de modelo lista para ser persistida por el DataContext.</returns>
        public static Purchase ToModelFromCreate(this PurchaseCreateDto dto)
        {
            return new Purchase
            {
                PurchaseNumber = dto.PurchaseNumber,
                ProjectName = dto.ProjectName,
                Requester = dto.Requester,
                Observations = dto.Observations,
                RequestDate = DateTime.UtcNow,
                Status = PurchaseStatuses.Received,
                
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

        /// <summary>
        /// Actualiza las propiedades de una entidad <see cref="Purchase"/> existente con los datos de un DTO.
        /// </summary>
        /// <remarks>
        /// Este método de extensión permite mutar el estado de la cabecera de la compra y actualiza 
        /// automáticamente la marca de tiempo 'UpdatedAt'.
        /// </remarks>
        /// <param name="purchase">La entidad original cargada con seguimiento (tracking).</param>
        /// <param name="dto">El DTO que contiene los nuevos valores.</param>
        public static void UpdateModel(this Purchase purchase, PurchaseUpdateDto dto)
        {
            purchase.ProjectName = dto.ProjectName;
            purchase.Requester = dto.Requester;
            purchase.Observations = dto.Observations;
            purchase.UpdatedAt = DateTime.UtcNow;
        }
    }
}