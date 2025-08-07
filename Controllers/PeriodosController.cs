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

        public async Task<IActionResult> Index(string busqueda, int? top)
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Auth");

            return await CargarListaView("Index", top, busqueda);
        }

        [HttpPost]
        public async Task<IActionResult> Create(decimal cantidadInicial, int? top, string busqueda)
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Auth");

            DateTime fechaInicio = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            DateTime fechaFinal = fechaInicio.AddMonths(1).AddDays(-1);

            string nombrePeriodo = fechaInicio.ToString("yyyyMM");
            bool existe = await _context.Periodos.AnyAsync(p => p.NombrePeriodo == nombrePeriodo);
            if (existe)
            {
                ViewBag.MensajeError = $"El periodo {nombrePeriodo} ya existe.";
                return await CargarListaView("Index", top, busqueda);
            }

            bool hayPeriodoAbierto = await _context.Periodos.AnyAsync(p => p.Estado == true);
            if (hayPeriodoAbierto)
            {
                ViewBag.MensajeError = "Debe cerrar todos los periodos antes de crear uno nuevo.";
                return await CargarListaView("Index", top, busqueda);
            }

            decimal cantidadPrestada = await _context.Prestamos
                .Where(p => p.Estado == true && p.FechaPrestamo >= fechaInicio && p.FechaPrestamo <= fechaFinal)
                .SumAsync(p => (decimal?)p.Cantidad) ?? 0;

            var nuevo = new Periodo
            {
                NombrePeriodo = nombrePeriodo,
                FechaInicio = fechaInicio,
                FechaFinal = fechaFinal,
                CantidadInicial = cantidadInicial,
                CantidadPrestada = cantidadPrestada,
                Estado = true,
                FechaCreacion = DateTime.Now
            };

            _context.Periodos.Add(nuevo);
            await _context.SaveChangesAsync();

            ViewBag.MensajeExito = "Periodo creado correctamente.";
            return await CargarListaView("Index", top, busqueda);
        }

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

            if (periodo.FechaFinal == null || DateTime.Now.Date < periodo.FechaFinal.Value.Date)
            {
                ViewBag.MensajeError = "No se puede cerrar el periodo antes de su fecha final.";
                return await CargarListaView("Index", top, busqueda);
            }

            periodo.Estado = false;
            periodo.FechaCierre = DateTime.Now;

            decimal totalIngresos = periodo.CantidadPrestada ?? 0;
            decimal totalEgresos = 0; 
            decimal balance = totalIngresos - totalEgresos;

            var cierre = new CierrePeriodo
            {
                IdPeriodo = periodo.IdPeriodo,
                TotalIngresos = totalIngresos,
                TotalEgresos = totalEgresos,
                Balance = balance,
                Comentarios = "",
                FechaCierre = periodo.FechaCierre
            };

            _context.CierrePeriodos.Add(cierre);
            await _context.SaveChangesAsync();

            ViewBag.MensajeExito = "Periodo cerrado correctamente.";
            return await CargarListaView("Index", top, busqueda);
        }

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

        [HttpGet]
        public async Task<IActionResult> DetalleCierre(int id)
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Auth");

            var cierre = await _context.CierrePeriodos
                .Include(c => c.Periodo)
                .FirstOrDefaultAsync(c => c.IdPeriodo == id);

            if (cierre == null)
                return NotFound();

            return PartialView("_DetalleCierre", cierre); // esto lo cargas como modal en Index
        }

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
