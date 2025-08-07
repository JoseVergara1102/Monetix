using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Monetix.Models
{
    public class PagoBeer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdPago { get; set; }

        [Required]
        public int NumRecibo { get; set; }

        [ForeignKey("Salida")]
        public int? IdSalida { get; set; }

        public decimal? CantidadPagada { get; set; }

        public decimal? CantidadDebe { get; set; }

        public bool? Estado { get; set; }

        public virtual Salida? Salida { get; set; }
    }
}
