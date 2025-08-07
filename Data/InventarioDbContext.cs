using Microsoft.EntityFrameworkCore;
using Monetix.Models;
using System.Threading;

namespace Monetix.Data
{
    public class InventarioDbContext : DbContext
    {
        public InventarioDbContext(DbContextOptions<InventarioDbContext> options)
            : base(options)
        {
        }

        public DbSet<ClienteBeer> ClientesBeer { get; set; }
        public DbSet<Barrio> Barrios { get; set; }
        public DbSet<Articulo> Articulos { get; set; }
        public DbSet<Entrada> Entradas { get; set; }
        public DbSet<Salida> Salidas { get; set; }
        public DbSet<Gasto> Gastos { get; set; }
        public DbSet<BalancePorPeriodo> BalancePorPeriodos { get; set; }
        public DbSet<VistaExistencia> VistaExistencias { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<VistaExistencia>()
                .HasNoKey()
                .ToView("Vista_Existencias");

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BalanceArticuloViewModel>()
                .HasNoKey()
                .ToView("Vista_BalanceArticulos");
        }

        public virtual DbSet<BalanceArticuloViewModel> VistaBalanceArticulos { get; set; }
        public DbSet<Precio> Precios { get; set; }
        public DbSet<PagoBeer> PagosBeer { get; set; }


    }
}
