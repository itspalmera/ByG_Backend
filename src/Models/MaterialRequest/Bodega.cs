using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ByG_Backend.src.Models.MaterialRequest
{
    [Table("bodegas")]
    public class Bodega
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        
        [JsonIgnore] 
        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
}