using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Monetix.Models
{
    [Table("salidas")]
    public class Salida
    {
        [Key]
        public int IdSalida { get; set; }

        [ForeignKey("Articulo")]
        public int? IdArticulo { get; set; }

        [ForeignKey("Cliente")]
        public int? IdCliente { get; set; }

        public int? Cantidad { get; set; }

        [StringLength(50)]
        public string? Medida { get; set; }

        public bool? Estado { get; set; }

        public int? CantidadUnidades { get; set; }

        public DateTime? FechaSalida { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal? MontoAPagar { get; set; }

        // Relaciones de navegación
        public virtual Articulo? Articulo { get; set; }

        public virtual ClienteBeer? Cliente { get; set; }
    }
}
