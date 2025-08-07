using Microsoft.EntityFrameworkCore;

namespace Monetix.Models
{
    [Keyless]
    public class BalancePorPeriodo
    {
        public int Periodo { get; set; }
        public decimal Balance { get; set; }
    }
}
