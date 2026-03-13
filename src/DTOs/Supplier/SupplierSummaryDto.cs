namespace ByG_Backend.src.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos para la representación resumida de un proveedor.
    /// Diseñado para optimizar el rendimiento en listados, tablas de búsqueda y selectores 
    /// donde solo se requiere la identificación básica y el estado.
    /// </summary>
    /// <param name="Id">Identificador único interno.</param>
    /// <param name="Rut">Rol Único Tributario para validación visual.</param>
    /// <param name="BusinessName">Razón Social de la empresa.</param>
    /// <param name="Email">Correo principal de contacto comercial.</param>
    /// <param name="ProductCategories">Rubros principales para filtrado rápido.</param>
    /// <param name="IsActive">Estado operativo del proveedor.</param>
    public record SupplierSummaryDto(
        int Id,
        string Rut,
        string BusinessName,
        string Email,
        string? ProductCategories,
        bool IsActive
    );
}