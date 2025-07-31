using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Monetix.Models
{
    [Table("cuotas")]
    public class Cuota
    {
        [Key]
        [Column("idCuota")]
        public int IdCuota { get; set; }

        [Column("montoAPagar")]
        [Display(Name = "Monto a Pagar")]
        [DataType(DataType.Currency)]
        public decimal? MontoAPagar { get; set; }

        [Column("fechaPagoCuota")]
        [Display(Name = "Fecha de Pago")]
        [DataType(DataType.DateTime)]
        public DateTime? FechaPagoCuota { get; set; }

        [Column("fechaVenceCuota")]
        [Display(Name = "Fecha de Vencimiento")]
        [DataType(DataType.DateTime)]
        public DateTime? FechaVenceCuota { get; set; }

        [Column("estado")]
        [Display(Name = "Estado de la Cuota")]
        public bool? Estado { get; set; }

        [Column("montoDebe")]
        [Display(Name = "Monto Pendiente")]
        [DataType(DataType.Currency)]
        public decimal? MontoDebe { get; set; }

        [Column("interes")]
        [Display(Name = "Interés")]
        [DataType(DataType.Currency)]
        public decimal? Interes { get; set; }

        [Column("deuda")]
        [Display(Name = "Deuda (Capital)")]
        [DataType(DataType.Currency)]
        public decimal? Deuda { get; set; }

        [Column("idPrestamo")]
        [Display(Name = "Préstamo Asociado")]
        public int? IdPrestamo { get; set; }

        [ForeignKey("IdPrestamo")]
        public Prestamo Prestamo { get; set; }

        public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
    }
}
