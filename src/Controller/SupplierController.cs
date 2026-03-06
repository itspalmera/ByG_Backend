using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using ByG_Backend.src.Data;
using ByG_Backend.src.DTOs;
using ByG_Backend.src.Helpers;
using ByG_Backend.src.RequestHelpers;
using ByG_Backend.src.Mappers;
using ByG_Backend.src.Models;

namespace ByG_Backend.src.Controller
{
    /// <summary>
    /// Controlador encargado de la gestión de Proveedores (Suppliers).
    /// Proporciona operaciones CRUD, búsqueda avanzada por categorías y validaciones de integridad
    /// para evitar duplicidad de registros.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SupplierController(DataContext context) : ControllerBase
    {
        /// <summary>
        /// Obtiene un listado paginado de proveedores con soporte para filtros de negocio y búsqueda dinámica.
        /// </summary>
        /// <remarks>
        /// Permite filtrar por estado (activo/inactivo), categorías de productos y rango de fechas de registro.
        /// La búsqueda dinámica abarca los campos RUT, Razón Social y Email.
        /// </remarks>
        /// <param name="queryParams">Parámetros que incluyen términos de búsqueda, filtros de categoría, estado y paginación.</param>
        /// <returns>Respuesta paginada con el resumen de proveedores (SupplierSummaryDto).</returns>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<SupplierSummaryDto>>>> GetSuppliers([FromQuery] SupplierQueryParameters queryParams)
        {
            try
            {
                var query = context.Supplier.AsNoTracking().AsQueryable();

                if (queryParams.IsActive.HasValue)
                {
                    query = query.Where(s => s.IsActive == queryParams.IsActive.Value);
                }

                if (!string.IsNullOrWhiteSpace(queryParams.ProductCategory))
                {
                    var category = queryParams.ProductCategory.ToLower();
                    query = query.Where(s => s.ProductCategories != null && s.ProductCategories.ToLower().Contains(category));
                }

                if (queryParams.StartDate.HasValue)
                {
                    query = query.Where(s => s.RegisteredAt >= queryParams.StartDate.Value);
                }
                
                if (queryParams.EndDate.HasValue)
                {
                    var endDate = queryParams.EndDate.Value.AddDays(1).AddTicks(-1);
                    query = query.Where(s => s.RegisteredAt <= endDate);
                }

                query = query.ApplySearch(queryParams.Search, "Rut", "BusinessName", "Email");

                query = query.ApplySorting(queryParams.SortBy, "RegisteredAt:desc");

                var pagedResult = await query.ToPagedResponseAsync(queryParams.PageNumber, queryParams.PageSize);

                // 5. PROYECCIÓN A DTO DE RESUMEN
                var dtos = pagedResult.Items.Select(s => new SupplierSummaryDto(
                    s.Id,
                    s.Rut,
                    s.BusinessName,
                    s.Email,
                    s.ProductCategories,
                    s.IsActive
                )).ToList();

                var response = new PagedResponse<SupplierSummaryDto>(
                    dtos, 
                    pagedResult.TotalItems, 
                    pagedResult.PageNumber, 
                    pagedResult.PageSize
                );

                return Ok(new ApiResponse<PagedResponse<SupplierSummaryDto>>(
                    true, 
                    "Listado de proveedores obtenido exitosamente.", 
                    response
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>(false, "Error: " + ex.Message));
            }
        }

        /// <summary>
        /// Obtiene la información detallada de un proveedor por su ID.
        /// </summary>
        /// <param name="id">ID único del proveedor.</param>
        /// <returns>Detalle del proveedor mapeado a SupplierDetailDto.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<SupplierDetailDto>>> GetSupplierById(int id)
        {
            var supplier = await context.Supplier
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (supplier == null)
            {
                return NotFound(new ApiResponse<SupplierDetailDto>(
                    false, 
                    "Proveedor no encontrado.", 
                    null, 
                    [$"No se encontró ningún proveedor con ID {id}."]
                ));
            }

            return Ok(new ApiResponse<SupplierDetailDto>(true, "Proveedor obtenido.", supplier.ToDetailDto()));
        }

        /// <summary>
        /// Registra un nuevo proveedor en el sistema tras validar que el RUT o Email no existan previamente.
        /// </summary>
        /// <param name="dto">DTO con la información necesaria para crear el proveedor.</param>
        /// <returns>El proveedor recién creado con su respectiva URI de ubicación.</returns>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<SupplierDetailDto>>> CreateSupplier([FromBody] SupplierCreateDto dto)
        {
            var existingSupplier = await context.Supplier
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Rut == dto.Rut || s.Email == dto.Email);

            if (existingSupplier != null)
            {
                return Conflict(new ApiResponse<SupplierDetailDto>(false, "El RUT o Email ya están registrados."));
            }

            var newSupplier = dto.ToModelFromCreate();
            context.Supplier.Add(newSupplier);
            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSupplierById), new { id = newSupplier.Id }, 
                new ApiResponse<SupplierDetailDto>(true, "Proveedor creado exitosamente.", newSupplier.ToDetailDto()));
        }

        /// <summary>
        /// Actualiza los datos de un proveedor existente, verificando que los nuevos RUT o Email no entren en conflicto con otros registros.
        /// </summary>
        /// <param name="id">ID del proveedor a actualizar.</param>
        /// <param name="dto">Nuevos datos del proveedor.</param>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<SupplierDetailDto>>> UpdateSupplier(int id, [FromBody] SupplierUpdateDto dto)
        {
            var supplier = await context.Supplier.FindAsync(id);

            if (supplier == null) return NotFound(new ApiResponse<string>(false, "No encontrado."));

            var duplicateExists = await context.Supplier
                .AsNoTracking()
                .AnyAsync(s => s.Id != id && (s.Rut == dto.Rut || s.Email == dto.Email));

            if (duplicateExists) return Conflict(new ApiResponse<string>(false, "RUT o Email ya en uso por otro proveedor."));

            supplier.UpdateModel(dto);
            await context.SaveChangesAsync();

            return Ok(new ApiResponse<SupplierDetailDto>(true, "Actualizado correctamente.", supplier.ToDetailDto()));
        }

        /// <summary>
        /// Cambia el estado de activación (IsActive) de un proveedor.
        /// </summary>
        /// <param name="id">ID del proveedor.</param>
        /// <returns>El nuevo estado de activación.</returns>
        [HttpPatch("{id}/toggle-status")]
        public async Task<ActionResult<ApiResponse<bool>>> ToggleSupplierStatus(int id)
        {
            var supplier = await context.Supplier.FindAsync(id);
            if (supplier == null) return NotFound(new ApiResponse<bool>(false, "No encontrado."));

            supplier.IsActive = !supplier.IsActive;
            await context.SaveChangesAsync();

            return Ok(new ApiResponse<bool>(true, $"Proveedor {(supplier.IsActive ? "activado" : "desactivado")}.", supplier.IsActive));
        }

        /// <summary>
        /// Elimina un proveedor de forma definitiva solo si no posee historial de cotizaciones o solicitudes asociadas.
        /// </summary>
        /// <param name="id">ID del proveedor a eliminar.</param>
        /// <returns>Confirmación de la eliminación o Conflicto si existen dependencias.</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteSupplier(int id)
        {
            bool hasHistory = await context.Supplier
                .AsNoTracking()
                .AnyAsync(s => s.Id == id && (s.Quotes.Count != 0 || s.RequestQuoteSuppliers.Count != 0));

            if (hasHistory)
            {
                return Conflict(new ApiResponse<string>(false, "No se puede eliminar: tiene historial asociado."));
            }

            int deletedRows = await context.Supplier.Where(s => s.Id == id).ExecuteDeleteAsync();
            if (deletedRows == 0) return NotFound();

            return Ok(new ApiResponse<string>(true, "Proveedor eliminado definitivamente."));
        }
    }
}