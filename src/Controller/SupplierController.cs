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
    [ApiController]
    [Route("api/[controller]")]
    public class SupplierController(DataContext context) : ControllerBase
    {
        // ============================================================
        // GET ALL - LISTADO PAGINADO (VERSION REFACTORIZADA)
        // ============================================================
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<SupplierSummaryDto>>>> GetSuppliers([FromQuery] SupplierQueryParameters queryParams)
        {
            try
            {
                var query = context.Supplier.AsNoTracking().AsQueryable();

                // 1. FILTROS ESPECÍFICOS (Lógica de negocio)
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

                // 2. BÚSQUEDA DINÁMICA (DRY)
                // Busca coincidencias en RUT, Razón Social y Email automáticamente
                query = query.ApplySearch(queryParams.Search, "Rut", "BusinessName", "Email");

                // 3. ORDENAMIENTO DINÁMICO (DRY)
                // Por defecto ordena por fecha de registro descendente
                query = query.ApplySorting(queryParams.SortBy, "RegisteredAt:desc");

                // 4. PAGINACIÓN GENÉRICA
                var pagedResult = await query.ToPagedResponseAsync(queryParams.PageNumber, queryParams.PageSize);

                // 5. MAPEO MANUAL A DTO (Items -> SummaryDto)
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

        // ============================================================
        // GET BY ID
        // ============================================================
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

        // ============================================================
        // CREAR PROVEEDOR
        // ============================================================
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

        // ============================================================
        // ACTUALIZAR PROVEEDOR
        // ============================================================
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

        // ============================================================
        // TOGGLE STATUS & DELETE
        // ============================================================
        [HttpPatch("{id}/toggle-status")]
        public async Task<ActionResult<ApiResponse<bool>>> ToggleSupplierStatus(int id)
        {
            var supplier = await context.Supplier.FindAsync(id);
            if (supplier == null) return NotFound(new ApiResponse<bool>(false, "No encontrado."));

            supplier.IsActive = !supplier.IsActive;
            await context.SaveChangesAsync();

            return Ok(new ApiResponse<bool>(true, $"Proveedor {(supplier.IsActive ? "activado" : "desactivado")}.", supplier.IsActive));
        }

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