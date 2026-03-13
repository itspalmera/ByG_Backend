using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ByG_Backend.src.Models.MaterialRequest
{
    [Table("productos")]
    public class Producto
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("bodega_id")]
        public int BodegaId { get; set; }
        public Bodega Bodega { get; set; } = null!;

        [Required]
        [Column("codigo_producto")]
        [MaxLength(50)]
        public string CodigoProducto { get; set; } = string.Empty;

        [Required]
        [Column("nombre_producto")]
        [MaxLength(255)]
        public string NombreProducto { get; set; } = string.Empty;

        [Column("ubicacion")]
        [MaxLength(100)]
        public string? Ubicacion { get; set; } // Pasillo, Estante, etc.

        [Column("talla_medida")]
        [MaxLength(100)]
        public string? TallaMedida { get; set; }

        [Column("formato")]
        [MaxLength(100)]
        public string? Formato { get; set; } // Equivalente a UnidadMedida

        [Column("cantidad")]
        public int Cantidad { get; set; } // Lo que antes llamábamos Stock

        [Column("observacion")]
        public string? Observacion { get; set; }

        [Column("activo")]
        public bool Activo { get; set; } = true;

        [Column("creado_en")]
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    }
}