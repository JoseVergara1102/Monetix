using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Monetix.Models
{
    [Table("prestamos")]
    public class Prestamo
    {
        [Key]
        [Column("idPrestamo")]
        public int IdPrestamo { get; set; }

        [Column("cantidad")]
        [Display(Name = "Monto del Préstamo")]
        [DataType(DataType.Currency)]
        public decimal? Cantidad { get; set; }

        [Column("fechaPrestamo")]
        [Display(Name = "Fecha del Préstamo")]
        [DataType(DataType.Date)]
        public DateTime? FechaPrestamo { get; set; }

        [Column("fechaVencimiento")]
        [Display(Name = "Fecha de Vencimiento")]
        [DataType(DataType.Date)]
        public DateTime? FechaVencimiento { get; set; }

        [Column("numCuotas")]
        [Display(Name = "Número de Cuotas")]
        public int? NumCuotas { get; set; }

        [Column("interes")]
        [Display(Name = "Interés (%)")]
        public decimal? Interes { get; set; }

        [Column("estado")]
        [Display(Name = "Activo")]
        public bool? Estado { get; set; }

        [Column("idCliente")]
        [Display(Name = "Cliente")]
        public int? IdCliente { get; set; }

        [ForeignKey("IdCliente")]
        public Cliente? Cliente { get; set; }
    }
}
