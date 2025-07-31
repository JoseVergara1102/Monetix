using Microsoft.EntityFrameworkCore;
using Monetix.Models;
using System.Collections.Generic;

namespace Monetix.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Periodo> Periodos { get; set; }
        public DbSet<Prestamo> Prestamos { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Banco> Bancos { get; set; }
        public DbSet<CuentaBancaria> CuentasBancarias { get; set; }
        public DbSet<GananciaPrestamo> GananciasXPrestamos { get; set; }
        public DbSet<Cuota> Cuotas { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<PeriodoPrestamo> PeriodoPrestamos { get; set; }
        public DbSet<CierrePeriodo> CierrePeriodos { get; set; }

    }
}
