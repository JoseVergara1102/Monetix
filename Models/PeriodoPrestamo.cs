using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Monetix.Models
{
    [Table("periodo_prestamos")]
    public class PeriodoPrestamo
    {
        [Key]
        [Column("idRegistro")]
        public int IdRegistro { get; set; }

        [ForeignKey("Periodo")]
        [Column("idPeriodo")]
        public int? IdPeriodo { get; set; }

        [ForeignKey("Prestamo")]
        [Column("idPrestamo")]
        public int? IdPrestamo { get; set; }

        [Column("fechaAsociacion")]
        public DateTime? FechaAsociacion { get; set; }

        // Relaciones
        public virtual Periodo? Periodo { get; set; }

        public virtual Prestamo? Prestamo { get; set; }
    }
}
