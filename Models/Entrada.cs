using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Monetix.Models
{
    [Table("entradas")]
    public class Entrada
    {
        [Key]
        public int IdEntrada { get; set; }

        [ForeignKey("Articulo")]
        public int? IdArticulo { get; set; }

        public int? Cantidad { get; set; }

        public int? CantidadUnidades { get; set; }

        [StringLength(50)]
        public string? Medida { get; set; }

        public DateTime? FechaEntrada { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal? MontoPagado { get; set; }
        public bool? Estado { get; set; }

        public virtual Articulo? Articulo { get; set; }
    }
}
