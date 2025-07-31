using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Monetix.Models
{
    [Table("bancos")]
    public class Banco
    {
        [Key]
        [Column("idBanco")]
        public int IdBanco { get; set; }

        [Column("codigo")]
        [StringLength(50)]
        public string? Codigo { get; set; }

        [Column("nombre")]
        [StringLength(50)]
        public string? Nombre { get; set; }

        [Column("cod_ins_fin")]
        [StringLength(50)]
        public string? CodInsFin { get; set; }

        [Column("estado")]
        public bool? Estado { get; set; }
    }
}
