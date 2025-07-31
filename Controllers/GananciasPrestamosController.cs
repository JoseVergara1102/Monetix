using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Monetix.Data;
using Monetix.Models;
using Microsoft.AspNetCore.Http; 

namespace Monetix.Controllers
{
    public class GananciasPrestamosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GananciasPrestamosController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? idPrestamo, int? top, string estado)
        {
            // Validar si hay usuario logueado (sesión activa)
            var idUsuario = HttpContext.Session.GetInt32("idUsuario");
            if (idUsuario == null)
            {
                return RedirectToAction("Login", "Auth"); 
            }

            int cantidad = top ?? 10;

            var query = _context.GananciasXPrestamos
                .Include(g => g.Prestamo)
                .ThenInclude(p => p.Cliente)
                .AsQueryable();

            // Filtro por ID de préstamo
            if (idPrestamo.HasValue)
            {
                query = query.Where(g => g.IdPrestamo == idPrestamo.Value);
            }

            // Filtro por estado textual
            if (!string.IsNullOrWhiteSpace(estado))
            {
                if (estado == "Pendiente")
                    query = query.Where(g => g.Prestamo.Estado == true);
                else if (estado == "Ganada")
                    query = query.Where(g => g.Prestamo.Estado == false);
            }

            var ganancias = await query
                .OrderByDescending(g => g.IdGanaciaPrestamo)
                .Take(cantidad)
                .Select(g => new GananciaPrestamo
                {
                    IdGanaciaPrestamo = g.IdGanaciaPrestamo,
                    CantidadPrestada = g.CantidadPrestada,
                    CantidadAPagar = g.CantidadAPagar,
                    Ganancia = g.Ganancia,
                    Estado = g.Prestamo != null && g.Prestamo.Estado == false,
                    IdPrestamo = g.IdPrestamo,
                    Prestamo = g.Prestamo
                })
                .ToListAsync();

            ViewBag.Top = cantidad;
            ViewBag.IdPrestamo = idPrestamo;
            ViewBag.EstadoFiltro = estado;

            return View(ganancias);
        }
    }
}
