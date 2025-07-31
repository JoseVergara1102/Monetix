using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Monetix.Data;
using Monetix.Models;
using Microsoft.AspNetCore.Http;

namespace Monetix.Controllers
{
    public class CuotasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CuotasController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? idPrestamo, int? top, string estado)
        {
            // Validar si hay usuario logueado
            var idUsuario = HttpContext.Session.GetInt32("idUsuario");
            if (idUsuario == null)
                return RedirectToAction("Login", "Auth");

            int cantidad = top ?? 10;

            var query = _context.Cuotas
                .Include(c => c.Prestamo)
                .ThenInclude(p => p.Cliente)
                .AsQueryable();

            // Filtrar por préstamo específico
            if (idPrestamo.HasValue)
            {
                query = query.Where(c => c.IdPrestamo == idPrestamo.Value);
            }

            // Filtrar por estado textual
            if (!string.IsNullOrWhiteSpace(estado))
            {
                if (estado == "Pendiente")
                    query = query.Where(c => c.Estado == false);
                else if (estado == "Pagada")
                    query = query.Where(c => c.Estado == true);
            }

            var cuotas = await query
                .OrderByDescending(c => c.IdCuota)
                .Take(cantidad)
                .ToListAsync();

            ViewBag.Top = cantidad;
            ViewBag.IdPrestamo = idPrestamo;
            ViewBag.EstadoFiltro = estado;

            return View(cuotas);
        }
    }
}
