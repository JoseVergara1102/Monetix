using System.ComponentModel.DataAnnotations;

namespace Monetix.Models
{
    public class Ayuda
    {
        [Key]
        public int IdAyuda { get; set; }

        [Display(Name = "Tipo")]
        [StringLength(10)]
        public string? Tipo { get; set; }

        [Display(Name = "Tabla")]
        [StringLength(20)]
        public string? Tabla { get; set; }

        [Display(Name = "Código")]
        [StringLength(10)]
        public string? CodDescripcion { get; set; }

        [Display(Name = "Descripción")]
        [StringLength(50)]
        public string? Descripcion { get; set; }

        [Display(Name = "Estado")]
        public bool? Estado { get; set; }
    }
}
