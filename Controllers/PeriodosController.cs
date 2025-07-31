using Microsoft.AspNetCore.Mvc;
using Monetix.Data;
using Monetix.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Monetix.Controllers
{
    public class PeriodosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PeriodosController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool UsuarioAutenticado()
        {
            return HttpContext.Session.GetInt32("idUsuario") != null;
        }

        // Vista principal
        public async Task<IActionResult> Index(string busqueda, int? top)
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Auth");

            return await CargarListaView("Index", top, busqueda);
        }

        // Crear nuevo periodo (desde modal)
        [HttpPost]
        public async Task<IActionResult> Create(decimal cantidadInicial, DateTime fechaInicio, DateTime fechaFinal, int? top, string busqueda)
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Auth");

            // Validación 1: No se permite fechaInicio < hoy
            if (fechaInicio.Date < DateTime.Today)
            {
                ViewBag.MensajeError = "La fecha de inicio no puede ser anterior a la fecha actual.";
                return await CargarListaView("Index", top, busqueda);
            }

            // Validación 2: No se puede repetir periodo (NombrePeriodo: yyyyMM)
            string nombrePeriodo = fechaInicio.ToString("yyyyMM", CultureInfo.InvariantCulture);
            bool existe = await _context.Periodos.AnyAsync(p => p.NombrePeriodo == nombrePeriodo);
            if (existe)
            {
                ViewBag.MensajeError = $"El periodo {nombrePeriodo} ya existe.";
                return await CargarListaView("Index", top, busqueda);
            }

            // Validación 3: Solo crear si todos los periodos están cerrados
            bool hayPeriodoAbierto = await _context.Periodos.AnyAsync(p => p.Estado == true);
            if (hayPeriodoAbierto)
            {
                ViewBag.MensajeError = "Debe cerrar todos los periodos antes de crear uno nuevo.";
                return await CargarListaView("Index", top, busqueda);
            }

            // Calcular cantidad prestada de préstamos en ese periodo
            decimal cantidadPrestada = await _context.Prestamos
                .Where(p => p.Estado == true && p.FechaPrestamo >= fechaInicio && p.FechaPrestamo <= fechaFinal)
                .SumAsync(p => (decimal?)p.Cantidad) ?? 0;

            // Crear nuevo periodo
            var nuevo = new Periodo
            {
                NombrePeriodo = nombrePeriodo,
                FechaInicio = fechaInicio,
                FechaFinal = fechaFinal,
                CantidadInicial = cantidadInicial,
                CantidadPrestada = cantidadPrestada,
                Estado = true
            };

            _context.Periodos.Add(nuevo);
            await _context.SaveChangesAsync();

            ViewBag.MensajeExito = "Periodo creado correctamente.";
            return await CargarListaView("Index", top, busqueda);
        }

        // Cerrar periodo
        [HttpPost]
        public async Task<IActionResult> Cerrar(int id, int? top, string busqueda)
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Auth");

            var periodo = await _context.Periodos.FindAsync(id);
            if (periodo == null)
            {
                ViewBag.MensajeError = "No se encontró el periodo.";
                return await CargarListaView("Index", top, busqueda);
            }

            periodo.Estado = false;
            await _context.SaveChangesAsync();

            ViewBag.MensajeExito = "Periodo cerrado correctamente.";
            return await CargarListaView("Index", top, busqueda);
        }

        // Reabrir periodo (si todos están cerrados)
        [HttpPost]
        public async Task<IActionResult> Reabrir(int id, int? top, string busqueda)
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Auth");

            bool todosCerrados = await _context.Periodos.AllAsync(p => p.Estado == false);
            if (!todosCerrados)
            {
                ViewBag.MensajeError = "Solo puede reabrirse un periodo si todos los demás están cerrados.";
                return await CargarListaView("Index", top, busqueda);
            }

            var periodo = await _context.Periodos.FindAsync(id);
            if (periodo == null)
            {
                ViewBag.MensajeError = "No se encontró el periodo.";
                return await CargarListaView("Index", top, busqueda);
            }

            periodo.Estado = true;
            await _context.SaveChangesAsync();

            ViewBag.MensajeExito = "Periodo reabierto correctamente.";
            return await CargarListaView("Index", top, busqueda);
        }

        // Método auxiliar para recargar la vista con filtros
        private async Task<IActionResult> CargarListaView(string viewName, int? top, string busqueda)
        {
            int cantidad = top ?? 10;

            var query = _context.Periodos.AsQueryable();

            if (!string.IsNullOrEmpty(busqueda))
                query = query.Where(p => p.NombrePeriodo.Contains(busqueda));

            var periodos = await query
                .OrderByDescending(p => p.IdPeriodo)
                .Take(cantidad)
                .ToListAsync();

            ViewBag.Top = cantidad;
            ViewBag.Busqueda = busqueda;
            ViewBag.TodoCerrado = await _context.Periodos.AllAsync(p => p.Estado == false);

            return View(viewName, periodos);
        }
    }
}
