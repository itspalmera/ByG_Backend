using ByG_Backend.src.DTOs;
using ByG_Backend.src.Models;

namespace ByG_Backend.src.Mappers
{
    /// <summary>
    /// Clase estática que proporciona métodos de extensión para el mapeo entre la entidad <see cref="Supplier"/> 
    /// y sus correspondientes Objetos de Transferencia de Datos (DTOs).
    /// Facilita la administración del ciclo de vida de los proveedores dentro del sistema.
    /// </summary>
    public static class SupplierMapper
    {
        /// <summary>
        /// Convierte una entidad de modelo <see cref="Supplier"/> en un <see cref="SupplierDetailDto"/>.
        /// </summary>
        /// <remarks>
        /// Formatea la fecha de registro a una cadena legible para el usuario final ("dd/MM/yyyy HH:mm:ss").
        /// </remarks>
        /// <param name="supplier">La entidad del proveedor recuperada desde la base de datos.</param>
        /// <returns>Un DTO con la información detallada del proveedor.</returns>
        public static SupplierDetailDto ToDetailDto(this Supplier supplier)
        {
            return new SupplierDetailDto(
                supplier.Id,
                supplier.Rut,
                supplier.BusinessName,
                supplier.ContactName,
                supplier.Email,
                supplier.Phone,
                supplier.Address,
                supplier.City,
                supplier.ProductCategories,
                supplier.RegisteredAt.ToString("dd/MM/yyyy HH:mm:ss"),
                supplier.IsActive
            );
        }

        /// <summary>
        /// Transforma un DTO de creación en una nueva instancia del modelo <see cref="Supplier"/>.
        /// </summary>
        /// <remarks>
        /// No asigna explícitamente 'RegisteredAt' ni 'IsActive', permitiendo que la base de datos 
        /// o las definiciones del modelo manejen sus valores por defecto.
        /// </remarks>
        /// <param name="dto">Datos de entrada para el nuevo proveedor.</param>
        /// <returns>Una entidad de modelo lista para ser insertada en el contexto.</returns>
        public static Supplier ToModelFromCreate(this SupplierCreateDto dto)
        {
            return new Supplier
            {
                Rut = dto.Rut,
                BusinessName = dto.BusinessName,
                ContactName = dto.ContactName,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address,
                City = dto.City,
                ProductCategories = dto.ProductCategories
            };
        }

        /// <summary>
        /// Actualiza un modelo de proveedor existente utilizando los datos de un <see cref="SupplierUpdateDto"/>.
        /// </summary>
        /// <remarks>
        /// Al ser un método 'void' que actúa sobre la referencia, permite que EF Core detecte 
        /// automáticamente los cambios en los objetos rastreados (Change Tracking).
        /// </remarks>
        /// <param name="supplier">La entidad original cargada en memoria.</param>
        /// <param name="dto">El DTO que contiene los nuevos valores a persistir.</param>
        public static void UpdateModel(this Supplier supplier, SupplierUpdateDto dto)
        {
            supplier.Rut = dto.Rut;
            supplier.BusinessName = dto.BusinessName;
            supplier.ContactName = dto.ContactName;
            supplier.Email = dto.Email;
            supplier.Phone = dto.Phone;
            supplier.Address = dto.Address;
            supplier.City = dto.City;
            supplier.ProductCategories = dto.ProductCategories;
            supplier.IsActive = dto.IsActive;
        }
    }
}