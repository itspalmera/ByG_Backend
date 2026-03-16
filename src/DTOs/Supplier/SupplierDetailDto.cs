namespace ByG_Backend.src.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos para la visualización detallada de un proveedor.
    /// Proporciona una vista completa de la ficha del proveedor, incluyendo su estado operativo
    /// y registros históricos de alta en el sistema.
    /// </summary>
    /// <param name="Id">Identificador único interno del proveedor.</param>
    /// <param name="Rut">Rol Único Tributario (RUT) de la entidad.</param>
    /// <param name="BusinessName">Razón Social o nombre legal de la empresa.</param>
    /// <param name="ContactName">Nombre de la persona de contacto directo.</param>
    /// <param name="Email">Correo electrónico para comunicaciones y licitaciones.</param>
    /// <param name="Phone">Número telefónico de contacto.</param>
    /// <param name="Address">Dirección física registrada.</param>
    /// <param name="City">Ciudad de origen o sucursal principal.</param>
    /// <param name="ProductCategories">Rubros o categorías de productos/servicios suministrados.</param>
    /// <param name="RegisteredAt">Fecha y hora de registro en la plataforma.</param>
    /// <param name="IsActive">Estado actual del proveedor (Habilitado/Deshabilitado para nuevas compras).</param>
    public record SupplierDetailDto(
        int Id,
        string Rut,
        string BusinessName,
        string? ContactName,
        string Email,
        string? Phone,
        string? Address,
        string? City,
        string? ProductCategories,
        string RegisteredAt,
        bool IsActive
    );
}