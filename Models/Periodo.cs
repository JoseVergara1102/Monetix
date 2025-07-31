using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Monetix.Models
{
    [Table("periodos")]
    public class Periodo
    {
        [Key]
        [Column("idPeriodo")]
        public int IdPeriodo { get; set; }

        [Column("cantidadInicial", TypeName = "decimal(18,0)")]
        public decimal? CantidadInicial { get; set; }

        [Column("fechaInicio")]
        [DataType(DataType.Date)]
        public DateTime? FechaInicio { get; set; }

        [Column("fechaFinal")]
        [DataType(DataType.Date)]
        public DateTime? FechaFinal { get; set; }

        [Column("periodo")]
        [StringLength(10)]
        public string? NombrePeriodo { get; set; }

        [Column("cantidadPrestada", TypeName = "decimal(18,2)")]
        public decimal? CantidadPrestada { get; set; }

        [Column("estado")]
        public bool? Estado { get; set; }

        [Column("fechaCreacion")]
        public DateTime? FechaCreacion { get; set; }

        [Column("fechaCierre")]
        public DateTime? FechaCierre { get; set; }
    }
}
