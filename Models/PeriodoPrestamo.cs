using Monetix.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("periodo_prestamos")]
public class PeriodoPrestamo
{
    [Key]
    public int IdPeriodoPrestamo { get; set; }

    [ForeignKey("Periodo")]
    public int IdPeriodo { get; set; }

    [ForeignKey("Prestamo")]
    public int IdPrestamo { get; set; }

    public DateTime FechaAsociacion { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Prestado { get; set; }

    public virtual Periodo Periodo { get; set; }
    public virtual Prestamo Prestamo { get; set; }
}
