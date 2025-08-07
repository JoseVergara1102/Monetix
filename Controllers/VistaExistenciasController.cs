using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Monetix.Data;
using Monetix.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Monetix.Controllers
{
    public class VistaExistenciasController : Controller
    {
        private readonly InventarioDbContext _context;

        public VistaExistenciasController(InventarioDbContext context)
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

        private async Task<IActionResult> CargarListaView(string viewName, int? top, string busqueda)
        {
            int cantidad = top ?? 10;

            var query = _context.VistaExistencias.AsQueryable();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                string b = busqueda.ToLower();
                query = query.Where(v =>
                    v.Descripcion.ToLower().Contains(b) ||
                    v.Codigo.ToLower().Contains(b)
                );
            }

            var lista = await query
                .OrderByDescending(v => v.CantidadDisponible)
                .Take(cantidad)
                .ToListAsync();

            ViewBag.Top = cantidad;
            ViewBag.Busqueda = busqueda;

            return View(viewName, lista);
        }
    }
}
