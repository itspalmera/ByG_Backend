using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ByG_Backend.src.Models.MaterialRequest
{
    public class Solicitud
    {
        [Key]
        public int Id { get; set; }
        
        
        public int Folio { get; set; }

        
        public string OrdenCompra { get; set; } = string.Empty;
        public string Proyecto { get; set; } = string.Empty;

       
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow; 
        public DateTime? FechaFinalizacion { get; set; }

        
        [ForeignKey("UsuarioSolicitante")]
        public string? UsuarioSolicitanteId { get; set; }
        public User? UsuarioSolicitante { get; set; }

       
        public EstadoSolicitud Estado { get; set; } = EstadoSolicitud.Pendiente;

        
        public string? Observaciones { get; set; }

        
        public List<DetalleSolicitud> Detalles { get; set; } = new List<DetalleSolicitud>();
    }
}