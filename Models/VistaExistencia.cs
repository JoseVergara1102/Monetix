namespace Monetix.Models
{
    public class VistaExistencia
    {
        public int IdArticulo { get; set; }
        public string Descripcion { get; set; }
        public string Codigo { get; set; }
        public int TotalEntradas { get; set; }
        public int TotalSalidas { get; set; }
        public int CantidadDisponible { get; set; }
        public int CantidadUnidadesDisponible { get; set; }
        public DateTime? UltimaEntrada { get; set; }
        public DateTime? UltimaSalida { get; set; }
    }
}
