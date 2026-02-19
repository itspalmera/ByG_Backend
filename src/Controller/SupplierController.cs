using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ByG_Backend.src.Data;
using ByG_Backend.src.DTOs;
using ByG_Backend.src.Helpers;
using ByG_Backend.src.Mappers;
using ByG_Backend.src.Models;
using ByG_Backend.src.Interfaces;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using ByG_Backend.src.DTOs.Supplier;

namespace ByG_Backend.src.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class SupplierController(DataContext context) : ControllerBase
    {
        private readonly DataContext _context = context;

        // VER Y BUSCAR (CON FILTROS Y ORDENAMIENTO)
        [HttpGet] // GET http://localhost:5280/api/supplier
        public async Task<ActionResult<ApiResponse<List<SupplierSummaryDto>>>> GetSuppliers([FromQuery] SupplierQueryParameters queryParams)
        {
            // 1. Iniciamos la consulta sin ejecutarla aún (IQueryable)
            var query = _context.Supplier.AsNoTracking().AsQueryable();

            // 2. BÚSQUEDA GLOBAL (Smart Search)
            if (!string.IsNullOrWhiteSpace(queryParams.Search))
            {
                // Convertimos a minúsculas para una búsqueda "Case-Insensitive" confiable
                var searchTerm = queryParams.Search.ToLower();
                
                // Buscamos simultáneamente en RUT, Nombre de Empresa y Email
                query = query.Where(s => 
                    s.Rut.ToLower().Contains(searchTerm) || 
                    s.BusinessName.ToLower().Contains(searchTerm) ||
                    s.Email.ToLower().Contains(searchTerm)
                );
            }

            // 3. FILTRO POR ESTADO (Activo / Inactivo)
            if (queryParams.IsActive.HasValue)
            {
                query = query.Where(s => s.IsActive == queryParams.IsActive.Value);
            }

            // 4. FILTRO POR CATEGORÍA DE PRODUCTO
            if (!string.IsNullOrWhiteSpace(queryParams.ProductCategory))
            {
                var category = queryParams.ProductCategory.ToLower();
                // Como 'ProductCategories' puede ser null, primero validamos que no lo sea
                query = query.Where(s => s.ProductCategories != null && s.ProductCategories.ToLower().Contains(category));
            }

            // 5. FILTRO POR RANGO DE FECHAS (RegisteredAt)
            if (queryParams.StartDate.HasValue)
            {
                query = query.Where(s => s.RegisteredAt >= queryParams.StartDate.Value);
            }
            
            if (queryParams.EndDate.HasValue)
            {
                // Buena práctica: Si el usuario manda "2026-02-18", queremos abarcar todo ese día hasta las 23:59:59.
                var endDate = queryParams.EndDate.Value.AddDays(1).AddTicks(-1);
                query = query.Where(s => s.RegisteredAt <= endDate);
            }

            // 6. ORDENAMIENTO DINÁMICO (Patrón Switch moderno de C#)
            query = queryParams.SortBy?.ToLower() switch
            {
                "name_asc" => query.OrderBy(s => s.BusinessName),
                "name_desc" => query.OrderByDescending(s => s.BusinessName),
                "date_asc" => query.OrderBy(s => s.RegisteredAt),
                "date_desc" => query.OrderByDescending(s => s.RegisteredAt),
                // Orden por defecto: Los más recientes primero
                _ => query.OrderByDescending(s => s.RegisteredAt) 
            };

            // 7. PROYECCIÓN Y EJECUCIÓN (Aquí recién golpeamos la Base de Datos)
            var suppliers = await query
                .Select(s => new SupplierSummaryDto(
                    s.Id,
                    s.Rut,
                    s.BusinessName,
                    s.Email,
                    s.ProductCategories,
                    s.IsActive
                ))
                .ToListAsync();

            return Ok(new ApiResponse<List<SupplierSummaryDto>>(
                success: true,
                message: "Listado de proveedores obtenido exitosamente.",
                data: suppliers
            ));
        }


        //VER POR ID
        [HttpGet("{id}")] // GET http://localhost:5280/api/supplier/1
        public async Task<ActionResult<ApiResponse<SupplierDetailDto>>> GetSupplierById(int id)
        {
            // 1. Búsqueda optimizada de solo lectura
            // AsNoTracking() es vital aquí para no sobrecargar la memoria de Entity Framework
            var supplier = await _context.Supplier
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            // 2. Manejo del caso donde no existe (HTTP 404)
            if (supplier == null)
            {
                return NotFound(new ApiResponse<SupplierDetailDto>(
                    success: false,
                    message: "Error al buscar el proveedor.",
                    errors: [$"No se encontró ningún proveedor registrado con el identificador {id}."]
                ));
            }

            // 3. Retornar HTTP 200 OK con los datos mappeados Modelo -> DTO
            return Ok(new ApiResponse<SupplierDetailDto>(
                success: true,
                message: "Proveedor obtenido exitosamente.",
                data: supplier.ToDetailDto() // Usamos el método para mapear a DTO de Detalle
            ));
        }



        //CREAR
        [HttpPost] // POST http://localhost:5280/api/supplier
        public async Task<ActionResult<ApiResponse<SupplierDetailDto>>> CreateSupplier([FromBody] SupplierCreateDto dto)
        {
            // 1. Regla de Negocio: Evitar duplicados por RUT o Email
            var existingSupplier = await _context.Supplier
                .AsNoTracking() // Optimización: AsNoTracking hace la consulta más rápida y consume menos memoria
                .FirstOrDefaultAsync(s => s.Rut == dto.Rut || s.Email == dto.Email);

            if (existingSupplier != null)
            {
                bool isRutDuplicate = existingSupplier.Rut == dto.Rut;
                string errorMsg = isRutDuplicate 
                    ? "Ya existe un proveedor registrado con este RUT." 
                    : "Ya existe un proveedor registrado con este Correo Electrónico.";

                return Conflict(new ApiResponse<SupplierDetailDto>(
                    success: false,
                    message: "Error al crear el proveedor.",
                    errors: [errorMsg]
                )); // Conflict (409) es el estado HTTP correcto para datos duplicados
            }


            // 2. Mapeo Manual (DTO -> Modelo)
            var newSupplier = dto.ToModelFromCreate(); // Usamos el método para mapear a Modelo

            // 3. Guardar en Base de Datos
            _context.Supplier.Add(newSupplier);
            await _context.SaveChangesAsync();

            // 4. Retornar 201 Created (Estándar REST) con tu ApiResponse y el DTO de Detalle del nuevo proveedor
            return CreatedAtAction(
                actionName: nameof(GetSupplierById), // Referencia al método GET por ID que haremos después
                routeValues: new { id = newSupplier.Id },
                value: new ApiResponse<SupplierDetailDto>(
                    success: true, 
                    message: "Proveedor creado exitosamente.", 
                    data: newSupplier.ToDetailDto() // Mapeamos el nuevo proveedor a DTO de Detalle para la respuesta
                )
            );
        }

        //EDITAR
        [HttpPut("{id}")] // PUT http://localhost:5280/api/supplier/1
        public async Task<ActionResult<ApiResponse<SupplierDetailDto>>> UpdateSupplier(int id, [FromBody] SupplierUpdateDto dto)
        {
            // 1. Buscar el proveedor (usamos FindAsync que por defecto hace tracking, necesario para el UPDATE)
            var supplier = await _context.Supplier.FindAsync(id);

            if (supplier == null)
            {
                return NotFound(new ApiResponse<SupplierDetailDto>(
                    success: false,
                    message: "Error al actualizar.",
                    errors: [$"No se encontró el proveedor con identificador {id}."]
                ));
            }

            // 2. Regla de Negocio: Validar que el nuevo RUT o Email no pertenezcan a OTRO proveedor
            // Usamos AnyAsync porque es más rápido que FirstOrDefaultAsync cuando solo queremos saber si existe (devuelve un booleano)
            var duplicateExists = await _context.Supplier
                .AsNoTracking()
                .AnyAsync(s => s.Id != id && (s.Rut == dto.Rut || s.Email == dto.Email));

            if (duplicateExists)
            {
                return Conflict(new ApiResponse<SupplierDetailDto>(
                    success: false,
                    message: "Error al actualizar el proveedor.",
                    errors: ["El RUT o Correo Electrónico ya está registrado en otro proveedor."]
                ));
            }




            // 3. Mapper para mutar la entidad existente
            supplier.UpdateModel(dto);

            // 4. Guardar los cambios (EF Core detecta las diferencias automáticamente y genera el UPDATE SQL)
            await _context.SaveChangesAsync();


            // 5. Retornar 200 OK con el DTO de Detalle actualizado para que el Frontend tenga la información más reciente
            return Ok(new ApiResponse<SupplierDetailDto>(
                success: true,
                message: "Proveedor actualizado exitosamente.",
                data: supplier.ToDetailDto() // Mapeamos el proveedor actualizado a DTO de Detalle para la respuesta
            ));
        }

        // CAMBIAR ESTADO (Soft Delete / Activar)
        [HttpPatch("{id}/toggle-status")] // PATCH http://localhost:5280/api/supplier/1/toggle-status
        public async Task<ActionResult<ApiResponse<bool>>> ToggleSupplierStatus(int id)
        {
            // 1. Buscar el proveedor rastreándolo para actualizarlo
            var supplier = await _context.Supplier.FindAsync(id);

            if (supplier == null)
            {
                return NotFound(new ApiResponse<bool>(
                    success: false,
                    message: "Error al cambiar el estado.",
                    errors: [$"No se encontró el proveedor con identificador {id}."]
                ));
            }

            // 2. Invertir el estado actual (si era true pasa a false, y viceversa)
            supplier.IsActive = !supplier.IsActive;

            // 3. Guardar cambios
            await _context.SaveChangesAsync();

            // 4. Mensaje dinámico para el Frontend
            string actionMessage = supplier.IsActive ? "activado" : "desactivado";

            // Retornamos el nuevo estado booleano para que el Frontend actualice la UI
            return Ok(new ApiResponse<bool>(
                success: true,
                message: $"Proveedor {actionMessage} exitosamente.",
                data: supplier.IsActive 
            ));
        }

        // ELIMINAR DEFINITIVAMENTE (Hard Delete)
        [HttpDelete("{id}")] // DELETE http://localhost:5280/api/supplier/1
        public async Task<ActionResult<ApiResponse<string>>> DeleteSupplier(int id)
        {
            // 1. Validar integridad referencial
            bool hasHistory = await _context.Supplier
                .AsNoTracking()
                .AnyAsync(s => s.Id == id && (s.Quotes.Count != 0 || s.RequestQuoteSuppliers.Count != 0));

            if (hasHistory)
            {
                return Conflict(new ApiResponse<string>( // Usamos <string> en lugar de <object>
                    success: false,
                    message: "No se puede eliminar el proveedor.",
                    errors: ["El proveedor tiene cotizaciones o historial asociado. Por integridad del sistema, utilice la desactivación (Soft Delete)."]
                ));
            }

            // 2. Eliminación ultra optimizada
            int deletedRows = await _context.Supplier
                .Where(s => s.Id == id)
                .ExecuteDeleteAsync();

            // 3. Verificar
            if (deletedRows == 0)
            {
                return NotFound(new ApiResponse<string>(
                    success: false,
                    message: "Error al eliminar.",
                    errors: [$"No se encontró el proveedor con identificador {id}."]
                ));
            }

            // 4. Retorno exitoso sin datos adicionales (data será null por defecto)
            return Ok(new ApiResponse<string>(
                success: true,
                message: "Proveedor eliminado definitivamente."
            ));
        }
    }


}