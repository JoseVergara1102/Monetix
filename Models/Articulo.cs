using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Monetix.Models
{
    [Table("articulos")]
    public class Articulo
    {
        [Key]
        [Column("idArticulo")]
        public int IdArticulo { get; set; }

        [Column("codigo")]
        [StringLength(10)]
        public string? Codigo { get; set; }

        [Column("descripcion")]
        [StringLength(50)]
        public string? Descripcion { get; set; }

        [Column("estado")]
        public bool? Estado { get; set; }
    }
}
