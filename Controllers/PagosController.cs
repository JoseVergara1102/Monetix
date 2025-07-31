using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Monetix.Data;
using Monetix.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Monetix.Controllers
{
    public class PagosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PagosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Pagos
        public async Task<IActionResult> Index(int top = 10)
        {
            var idUsuario = HttpContext.Session.GetInt32("idUsuario");
            if (idUsuario == null)
                return RedirectToAction("Login", "Auth");

            // Listado de pagos con cuota y préstamo relacionados
            var pagos = await _context.Pagos
                .Include(p => p.Cuota)
                .ThenInclude(c => c.Prestamo)
                .OrderByDescending(p => p.FechaPago)
                .Take(top)
                .ToListAsync();

            // Cuotas pendientes (para formulario de crear pago)
            var cuotasDisponibles = await _context.Cuotas
                .Include(c => c.Prestamo)
                .Where(c => c.Estado != true && c.MontoDebe > 0)
                .OrderBy(c => c.FechaVenceCuota)
                .ToListAsync();

            ViewBag.Cuotas = cuotasDisponibles;
            ViewBag.Top = top;

            return View(pagos);
        }

        // POST: Pagos/Create
        [HttpPost]
        public async Task<IActionResult> Create(decimal? montoPagado, int? idCuota, string medioPago)
        {
            if (idCuota == null || montoPagado == null || montoPagado <= 0)
            {
                ViewBag.MensajeError = "Debe seleccionar una cuota válida y un monto mayor a cero.";
                return await RecargarVistaIndex();
            }

            var cuota = await _context.Cuotas
                .Include(c => c.Prestamo)
                .FirstOrDefaultAsync(c => c.IdCuota == idCuota);

            if (cuota == null)
            {
                ViewBag.MensajeError = "La cuota seleccionada no existe.";
                return await RecargarVistaIndex();
            }

            if (cuota.Estado == true)
            {
                ViewBag.MensajeError = "Esta cuota ya fue pagada.";
                return await RecargarVistaIndex();
            }

            decimal monto = montoPagado ?? 0;
            decimal interes = cuota.Prestamo?.Interes ?? 0; // porcentaje
            decimal deudaTotal = cuota.MontoDebe ?? 0;

            if (monto > deudaTotal)
            {
                ViewBag.MensajeError = "El monto no puede ser mayor al valor de la deuda.";
                return await RecargarVistaIndex();
            }

            // Calcular interés sobre el capital original de la cuota
            decimal montoOriginal = cuota.MontoAPagar ?? 0;
            decimal montoInteres = Math.Round((montoOriginal * interes) / 100, 2);

            // Calcular abonos
            decimal abonoInteres = Math.Min(monto, montoInteres);
            decimal abonoCapital = Math.Max(0, monto - abonoInteres);

            // Registrar el pago
            var nuevoPago = new Pago
            {
                IdCuota = cuota.IdCuota,
                MontoPagado = monto,
                MedioPago = medioPago,
                FechaPago = DateTime.Now,
                Estado = true,
                AInteres = abonoInteres,
                ACapital = abonoCapital
            };

            // Restar monto de la deuda
            cuota.MontoDebe = Math.Round((cuota.MontoDebe ?? 0) - monto, 2);

            // Si ya no debe nada, marcar como pagada
            if (cuota.MontoDebe <= 0)
                cuota.Estado = true;

            try
            {
                _context.Pagos.Add(nuevoPago);
                _context.Cuotas.Update(cuota);
                await _context.SaveChangesAsync();

                // Actualizar préstamo si todas las cuotas están pagadas
                if (cuota.IdPrestamo.HasValue)
                    await VerificarYCambiarEstadoPrestamo(cuota.IdPrestamo.Value);

                ViewBag.MensajeExito = "Pago registrado exitosamente.";
            }
            catch (Exception ex)
            {
                ViewBag.MensajeError = "Error al registrar el pago: " + ex.Message;
            }

            return await RecargarVistaIndex();
        }

        // Método para verificar si un préstamo puede cerrarse
        private async Task VerificarYCambiarEstadoPrestamo(int idPrestamo)
        {
            var cuotas = await _context.Cuotas
                .Where(c => c.IdPrestamo == idPrestamo)
                .ToListAsync();

            var prestamo = await _context.Prestamos.FindAsync(idPrestamo);
            if (prestamo == null) return;

            bool hayPendientes = cuotas.Any(c => c.Estado == false || c.MontoDebe > 0);
            prestamo.Estado = hayPendientes;

            _context.Prestamos.Update(prestamo);
            await _context.SaveChangesAsync();
        }

        // Recarga el Index
        private async Task<IActionResult> RecargarVistaIndex()
        {
            var pagos = await _context.Pagos
                .Include(p => p.Cuota)
                .ThenInclude(c => c.Prestamo)
                .OrderByDescending(p => p.FechaPago)
                .Take(10)
                .ToListAsync();

            var cuotas = await _context.Cuotas
                .Include(c => c.Prestamo)
                .Where(c => c.Estado != true && c.MontoDebe > 0)
                .OrderBy(c => c.FechaVenceCuota)
                .ToListAsync();

            ViewBag.Cuotas = cuotas;
            ViewBag.Top = 10;

            return View("Index", pagos);
        }
    }
}
