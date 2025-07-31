using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Monetix.Models
{
    [Table("roles")]
    public class Rol
    {
        [Key]
        [Column("idRol")]
        public int IdRol { get; set; }

        [Column("nombre")]
        [StringLength(50)]
        public string? Nombre { get; set; }

        [Column("estado")]
        public bool? Estado { get; set; }
    }
}
