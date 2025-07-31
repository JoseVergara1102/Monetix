using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Monetix.Models
{
    [Table("cierre_periodos")]
    public class CierrePeriodo
    {
        [Key]
        [Column("idCierre")]
        public int IdCierre { get; set; }

        [ForeignKey("Periodo")]
        [Column("idPeriodo")]
        public int? IdPeriodo { get; set; }

        [Column("totalIngresos", TypeName = "decimal(18,2)")]
        public decimal? TotalIngresos { get; set; }

        [Column("totalEgresos", TypeName = "decimal(18,2)")]
        public decimal? TotalEgresos { get; set; }

        [Column("balance", TypeName = "decimal(18,2)")]
        public decimal? Balance { get; set; }

        [Column("comentarios")]
        [StringLength(1000)]
        public string? Comentarios { get; set; }

        [Column("fechaCierre")]
        public DateTime? FechaCierre { get; set; }

        // Relación con Periodo
        public virtual Periodo? Periodo { get; set; }
    }
}
