using System.ComponentModel.DataAnnotations;

namespace ByG_Backend.src.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos para la actualización de la información básica 
    /// de una Solicitud de Compra.
    /// </summary>
    /// <param name="ProjectName">Nombre del proyecto o centro de costo actualizado.</param>
    /// <param name="Requester">Nombre de la persona o departamento que solicita el requerimiento.</param>
    /// <param name="Observations">Notas o aclaraciones adicionales sobre la modificación.</param>
    public record PurchaseUpdateDto(
        [Required(ErrorMessage = "El nombre del proyecto es obligatorio.")] 
        string ProjectName,
        
        [Required(ErrorMessage = "El nombre del solicitante es obligatorio.")] 
        string Requester,
        
        string? Observations
        // Nota: El cambio de estado (Status) se maneja a través de endpoints de transición 
        // de workflow para asegurar la trazabilidad y reglas de negocio.
    );
}