namespace ByG_Backend.src.Models.MaterialRequest
{
    public enum EstadoSolicitud
    {
        Pendiente,          // Recién creada por el Supervisor, nadie la ha visto.
        EnRevision,         // El Bodeguero abrió el pedido y está contando stock.
        AprobadaBodega,     // Stock reservado y listo para retiro.
        RequiereCompra,     // No hay stock, se derivó a Adquisiciones.
        Finalizada,         // Materiales entregados en mano.
        Rechazada           // Denegada (por falta de presupuesto o error).
    }
}