using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Monetix.Models
{
    [Table("clientesbeer")]
    public class ClienteBeer
    {
        [Key]
        [Column("idCliente")]
        public int IdCliente { get; set; }

        [Column("primerNom")]
        [StringLength(20)]
        public string? PrimerNom { get; set; }

        [Column("segundoNom")]
        [StringLength(20)]
        public string? SegundoNom { get; set; }

        [Column("primerApe")]
        [StringLength(20)]
        public string? PrimerApe { get; set; }

        [Column("segundoApe")]
        [StringLength(20)]
        public string? SegundoApe { get; set; }

        [Column("direccion")]
        [StringLength(50)]
        public string? Direccion { get; set; }

        [Column("idBarrio")]
        public int? IdBarrio { get; set; }

        [ForeignKey("IdBarrio")]
        public virtual Barrio? Barrio { get; set; }

        [Column("estado")]
        public bool? Estado { get; set; }
    }
}
