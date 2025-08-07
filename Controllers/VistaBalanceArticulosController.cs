using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Monetix.Data;
using Monetix.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Monetix.Controllers
{
    public class VistaBalanceArticulosController : Controller
    {
        private readonly InventarioDbContext _context;

        public VistaBalanceArticulosController(InventarioDbContext context)
        {
            _context = context;
        }

        private bool UsuarioAutenticado()
        {
            return HttpContext.Session.GetInt32("idUsuario") != null;
        }

        public async Task<IActionResult> Index(string busqueda, int? top, int? anio, int? mes)
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Auth");

            return await CargarListaView("Index", top, busqueda, anio, mes);
        }

        private async Task<IActionResult> CargarListaView(string viewName, int? top, string busqueda, int? anio, int? mes)
        {
            int cantidad = top ?? 10;

            var query = _context.VistaBalanceArticulos.AsQueryable();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                string b = busqueda.ToLower();
                query = query.Where(v =>
                    v.Descripcion.ToLower().Contains(b) ||
                    v.Codigo.ToLower().Contains(b)
                );
            }

            if (anio.HasValue)
                query = query.Where(v => v.Periodo / 100 == anio.Value);

            if (mes.HasValue)
                query = query.Where(v => v.Periodo % 100 == mes.Value);

            var lista = await query
                .OrderByDescending(v => v.Periodo)
                .ThenByDescending(v => v.GananciaBruta)
                .Take(cantidad)
                .ToListAsync();

            ViewBag.Top = cantidad;
            ViewBag.Busqueda = busqueda;
            ViewBag.Anio = anio;
            ViewBag.Mes = mes;

            return View(viewName, lista);
        }
    }
}
