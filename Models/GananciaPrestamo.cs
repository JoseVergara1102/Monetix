using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Monetix.Models
{
    [Table("ganaciasXPrestamos")]
    public class GananciaPrestamo
    {
        [Key]
        [Column("idGanaciaPrestamo")]
        public int IdGanaciaPrestamo { get; set; }

        [Column("cantidadPrestada")]
        [DataType(DataType.Currency)]
        public decimal? CantidadPrestada { get; set; }

        [Column("cantidadaPagar")]
        [DataType(DataType.Currency)]
        public decimal? CantidadAPagar { get; set; }

        [Column("ganancia")]
        [DataType(DataType.Currency)]
        public decimal? Ganancia { get; set; }

        [Column("estado")]
        public bool? Estado { get; set; }

        [Column("idPrestamo")]
        public int? IdPrestamo { get; set; }

        [ForeignKey("IdPrestamo")]
        public virtual Prestamo? Prestamo { get; set; }
    }
}
