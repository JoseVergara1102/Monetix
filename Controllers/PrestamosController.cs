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
    public class PrestamosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PrestamosController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool UsuarioAutenticado()
        {
            return HttpContext.Session.GetInt32("idUsuario") != null;
        }

        public async Task<IActionResult> Index(string busqueda, int? top)
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Auth");

            int cantidad = top ?? 10;

            var query = _context.Prestamos
                                .Include(p => p.Cliente)
                                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                query = query.Where(p =>
                    (p.Cliente.PrimerNom + " " + p.Cliente.PrimerApe + " " + p.Cliente.SegundoApe).Contains(busqueda));
            }

            var lista = await query
                .OrderByDescending(p => p.FechaPrestamo)
                .Take(cantidad)
                .ToListAsync();

            ViewBag.Clientes = await _context.Clientes
                .Where(c => c.Estado == true)
                .OrderBy(c => c.PrimerNom)
                .ToListAsync();

            ViewBag.Top = cantidad;
            ViewBag.Busqueda = busqueda;

            return View(lista);
        }

        [HttpPost]
        public async Task<IActionResult> Create(decimal cantidad, DateTime fechaVencimiento, int numCuotas, int idCliente)
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Auth");

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.IdCliente == idCliente && c.Estado == true);
            if (cliente == null)
            {
                TempData["MensajeError"] = "Cliente no válido o inactivo.";
                return RedirectToAction("Index");
            }

            bool tienePrestamoActivo = await _context.Prestamos
                .AnyAsync(p => p.IdCliente == idCliente && p.Estado == true);

            if (tienePrestamoActivo)
            {
                TempData["MensajeError"] = "El cliente ya tiene un préstamo activo.";
                return RedirectToAction("Index");
            }

            decimal interes = cantidad < 100000 ? 20m : 10m;
            var interesDecimal = interes / 100m;

            var cantidadAPagar = cantidad + (cantidad * interesDecimal);
            var ganancia = cantidadAPagar - cantidad;

            var prestamo = new Prestamo
            {
                Cantidad = cantidad,
                FechaPrestamo = DateTime.Now,
                FechaVencimiento = fechaVencimiento,
                NumCuotas = numCuotas,
                Interes = interes,
                Estado = true,
                IdCliente = idCliente
            };

            _context.Prestamos.Add(prestamo);
            await _context.SaveChangesAsync();

            var nuevaGanancia = new GananciaPrestamo
            {
                CantidadPrestada = cantidad,
                CantidadAPagar = cantidadAPagar,
                Ganancia = ganancia,
                Estado = false,
                IdPrestamo = prestamo.IdPrestamo
            };

            _context.GananciasXPrestamos.Add(nuevaGanancia);

            // Generar cuotas según nuevo comportamiento
            decimal deudaActual = cantidad;
            decimal deudaPorCuota = cantidad / numCuotas;
            DateTime fechaBase = prestamo.FechaPrestamo ?? DateTime.Now;

            for (int i = 0; i < numCuotas; i++)
            {
                decimal interesCuota = deudaActual * interesDecimal;
                decimal montoAPagar = deudaActual + interesCuota;

                var cuota = new Cuota
                {
                    Deuda = deudaActual,
                    Interes = interesCuota,
                    MontoAPagar = montoAPagar,
                    MontoDebe = montoAPagar,
                    FechaVenceCuota = fechaBase.AddMonths(i + 1),
                    FechaPagoCuota = null,
                    Estado = false,
                    IdPrestamo = prestamo.IdPrestamo
                };

                _context.Cuotas.Add(cuota);

                // Actualizar la deuda para la próxima cuota
                deudaActual -= deudaPorCuota;
            }

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "Préstamo y cuotas generados correctamente.";
            return RedirectToAction("Index");
        }

        //Desactivar y activar préstamos
        [HttpPost]
        public async Task<IActionResult> Desactivar(int id)
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Auth");

            var prestamo = await _context.Prestamos.FindAsync(id);
            if (prestamo == null)
            {
                TempData["MensajeError"] = "Préstamo no encontrado.";
                return RedirectToAction("Index");
            }

            prestamo.Estado = false;
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "Préstamo desactivado correctamente.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Activar(int id)
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Auth");

            var prestamo = await _context.Prestamos.FindAsync(id);
            if (prestamo == null)
            {
                TempData["MensajeError"] = "Préstamo no encontrado.";
                return RedirectToAction("Index");
            }

            prestamo.Estado = true;
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "Préstamo activado correctamente.";
            return RedirectToAction("Index");
        }

        // Ampliar plazo de un préstamo
        [HttpPost]
        public async Task<IActionResult> AmpliarPlazo(int idPrestamo, int diasAmpliar)
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Auth");

            var prestamo = await _context.Prestamos.FindAsync(idPrestamo);
            if (prestamo == null)
            {
                TempData["MensajeError"] = "Préstamo no encontrado.";
                return RedirectToAction("Index");
            }

            if (!prestamo.Estado.HasValue || prestamo.Estado == false)
            {
                TempData["MensajeError"] = "Solo se puede ampliar el plazo de préstamos activos.";
                return RedirectToAction("Index");
            }

            if (!prestamo.FechaVencimiento.HasValue)
            {
                TempData["MensajeError"] = "El préstamo no tiene una fecha de vencimiento válida.";
                return RedirectToAction("Index");
            }

            try
            {
                prestamo.FechaVencimiento = prestamo.FechaVencimiento.Value.AddDays(diasAmpliar);

                // Marcar la propiedad como modificada explícitamente
                _context.Entry(prestamo).Property(p => p.FechaVencimiento).IsModified = true;

                await _context.SaveChangesAsync();

                TempData["MensajeExito"] = $"Plazo ampliado correctamente en {diasAmpliar} días.";
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = $"Error al ampliar plazo: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
        private async Task VerificarYCerrarPrestamoSiPagado(int idPrestamo)
        {
            var cuotas = await _context.Cuotas
                .Where(c => c.IdPrestamo == idPrestamo)
                .ToListAsync();

            if (cuotas.All(c => c.Estado == true))
            {
                var prestamo = await _context.Prestamos.FindAsync(idPrestamo);
                if (prestamo != null && prestamo.Estado == true)
                {
                    prestamo.Estado = false; 
                    _context.Prestamos.Update(prestamo);
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}
