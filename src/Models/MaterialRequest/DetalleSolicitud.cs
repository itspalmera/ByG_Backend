using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ByG_Backend.src.Models.MaterialRequest
{
    public class DetalleSolicitud
    {
        [Key]
        public int Id { get; set; }

        public int SolicitudId { get; set; }
        [ForeignKey("SolicitudId")]
        public Solicitud Solicitud { get; set; } = null!;

        public int? ProductoId { get; set; }
        [ForeignKey("ProductoId")]
        public Producto? Producto { get; set; }

        // Datos del ítem manual (cuando no hay stock)
        public string? TemporalNombre { get; set; }
        public string? TemporalCodigo { get; set; }
        public string? TemporalUnidad { get; set; }
        public string? TemporalTalla { get; set; }
        
    
        public string? Observacion { get; set; } 

        public int CantidadSolicitada { get; set; }
        public int CantidadAprobada { get; set; }
    }
}