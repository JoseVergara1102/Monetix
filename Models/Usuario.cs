using Monetix.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Monetix.Models
{
    [Table("usuarios")]
    public class Usuario
    {
        [Key]
        public int IdUsuario { get; set; }

        [Required]
        [Column("usuario")]
        [MaxLength(50)]
        public string UsuarioNombre { get; set; }

        [Required]
        [Column("pass")]
        [MaxLength(50)]
        public string Contrasena { get; set; }

        [ForeignKey("Rol")]
        public int IdRol { get; set; }

        public bool Estado { get; set; }

        // Relación con Rol
        public Rol Rol { get; set; }
    }
}
