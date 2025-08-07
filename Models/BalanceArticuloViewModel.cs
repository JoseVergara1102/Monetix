using Microsoft.EntityFrameworkCore;

namespace Monetix.Models
{
    [Keyless]
    public class BalanceArticuloViewModel
    {
        public int IdArticulo { get; set; }
        public string Codigo { get; set; }
        public string Descripcion { get; set; }

        public int Periodo { get; set; }
        public string NombreMes { get; set; }

        public int TotalEntradas { get; set; }
        public int TotalUnidadesEntrada { get; set; }
        public decimal TotalComprado { get; set; }

        public int TotalSalidas { get; set; }
        public int TotalUnidadesSalida { get; set; }
        public decimal TotalVendido { get; set; }

        public decimal GananciaBruta { get; set; }
    }
}
