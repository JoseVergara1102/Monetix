using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Monetix.Models
{
    [Table("precios")]
    public class Precio
    {
        [Key]
        public int IdPrecio { get; set; }

        [ForeignKey("Articulo")]
        public int? IdArticulo { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal? PrecioValor { get; set; }

        public bool? Estado { get; set; }

        [StringLength(50)]
        public string? Medida { get; set; }

        public virtual Articulo? Articulo { get; set; }
    }
}
