using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Monetix.Models
{
    [Table("pagos")]
    public class Pago
    {
        [Key]
        [Column("idPago")]
        public int IdPago { get; set; }

        [Column("montoPagado")]
        [Display(Name = "Monto Pagado")]
        [DataType(DataType.Currency)]
        public decimal? MontoPagado { get; set; }

        [Column("medioPago")]
        [StringLength(50)]
        [Display(Name = "Medio de Pago")]
        public string MedioPago { get; set; }

        [Column("fechaPago")]
        [Display(Name = "Fecha de Pago")]
        [DataType(DataType.DateTime)]
        public DateTime? FechaPago { get; set; }

        [Column("aCapital")]
        [Display(Name = "Abono a Capital")]
        public decimal? ACapital { get; set; }

        [Column("aInteres")]
        [Display(Name = "Abono a Interés")]
        public decimal? AInteres { get; set; }

        [Column("estado")]
        [Display(Name = "Estado")]
        public bool? Estado { get; set; }

        [Column("idCuota")]
        [Display(Name = "Cuota Asociada")]
        public int? IdCuota { get; set; }

        [ForeignKey("IdCuota")]
        public Cuota Cuota { get; set; }
    }
}
