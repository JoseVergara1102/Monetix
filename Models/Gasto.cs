using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Monetix.Models
{
    [Table("gastos")]
    public class Gasto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("idGasto")]
        public int IdGasto { get; set; }

        [Column("descripcionGasto")]
        public string DescripcionGasto { get; set; }

        [Column("cantidad")]
        public int? Cantidad { get; set; }

        [Column("montoGastado")]
        [DataType(DataType.Currency)]
        public decimal? MontoGastado { get; set; }

        [Column("fechaGasto")]
        [DataType(DataType.DateTime)]
        public DateTime? FechaGasto { get; set; }
    }
}
