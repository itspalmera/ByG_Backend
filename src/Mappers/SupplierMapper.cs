using ByG_Backend.src.DTOs;
using ByG_Backend.src.Models;

namespace ByG_Backend.src.Mappers
{
    public static class SupplierMapper
    {
        // 1. Modelo -> DTO (Para ver detalles)
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
                supplier.RegisteredAt,
                supplier.IsActive
            );
        }

        // 2. DTO -> Modelo (Para Crear)
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
                // RegisteredAt e IsActive toman sus valores por defecto del Model
            };
        }

        // 3. Mutar Modelo existente desde DTO (Para Editar)
        // Usamos 'void' porque estamos modificando el objeto que EF Core ya está rastreando en memoria
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