using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization; // 👈 Asegúrate de agregar esto

namespace Monetix.Models
{
    [Table("barrios")]
    public class Barrio
    {
        [Key]
        [Column("idBarrio")]
        public int IdBarrio { get; set; }

        [Column("nombre")]
        [StringLength(50)]
        public string? Nombre { get; set; }

        [Column("estado")]
        public bool? Estado { get; set; }

        // Relación inversa: Lista de clientes en el barrio
        [JsonIgnore] // 👈 Esto evita el ciclo
        public virtual ICollection<ClienteBeer>? ClientesBeer { get; set; }
    }
}
