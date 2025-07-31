using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Monetix.Models
{
    [Table("cuentasBancarias")]
    public class CuentaBancaria
    {
        [Key]
        [Column("idCuenta")]
        public int IdCuenta { get; set; }

        [Column("numCuenta")]
        [StringLength(30)]
        [Display(Name = "Número de Cuenta")]
        public string? NumCuenta { get; set; }

        [Column("tipoCuenta")]
        [StringLength(30)]
        [Display(Name = "Tipo de Cuenta")]
        public string? TipoCuenta { get; set; }

        [Column("estado")]
        [Display(Name = "Estado")]
        public bool? Estado { get; set; }

        [Column("idBanco")]
        [Display(Name = "Banco")]
        public int? IdBanco { get; set; }

        [Column("idCliente")]
        [Display(Name = "Cliente")]
        public int? IdCliente { get; set; }

        // Relaciones

        [ForeignKey("IdBanco")]
        public Banco? Banco { get; set; }

        [ForeignKey("IdCliente")]
        public Cliente? Cliente { get; set; }
    }
}
