using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Monetix.Models
{
    [Table("clientes")]
    public class Cliente
    {
        [Key]
        [Column("idCliente")]
        public int IdCliente { get; set; }

        [Column("primerNom")]
        [StringLength(20)]
        public string? PrimerNom { get; set; }

        [Column("segundoNom")]
        [StringLength(20)]
        public string? SegundoNom { get; set; }

        [Column("primerApe")]
        [StringLength(20)]
        public string? PrimerApe { get; set; }

        [Column("segundoApe")]
        [StringLength(20)]
        public string? SegundoApe { get; set; }

        [Column("telefono")]
        public string? Telefono { get; set; }

        [Column("direccion")]
        [StringLength(50)]
        public string? Direccion { get; set; }

        [Column("barrio")]
        [StringLength(50)]
        public string? Barrio { get; set; }

        [Column("email")]
        [StringLength(50)]
        [EmailAddress(ErrorMessage = "Correo electrónico no válido")]
        public string? Email { get; set; }

        [Column("estado")]
        public bool? Estado { get; set; }

        [Column("fechaRegistro")]
        public DateTime? FechaRegistro { get; set; }
    }
}
