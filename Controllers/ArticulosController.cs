using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Monetix.Data;
using Monetix.Models;
using System.Linq;
using System.Threading.Tasks;

namespace MonetixBeer.Controllers
{
    public class ArticulosController : Controller
    {
        private readonly InventarioDbContext _context;

        public ArticulosController(InventarioDbContext context)
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
        public async Task<IActionResult> Create(string codigo, string descripcion, int? top, string busqueda)
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Auth");

            if (string.IsNullOrWhiteSpace(codigo) || string.IsNullOrWhiteSpace(descripcion))
            {
                ViewBag.MensajeError = "Código y descripción son obligatorios.";
                ViewBag.AbrirModal = "true";
                return await CargarListaView("Index", top, busqueda);
            }

            bool existe = await _context.Articulos
                .AnyAsync(a => a.Codigo.ToLower() == codigo.ToLower());

            if (existe)
            {
                ViewBag.MensajeError = "Ya existe un artículo con ese código.";
                ViewBag.AbrirModal = "true";
                return await CargarListaView("Index", top, busqueda);
            }

            var nuevo = new Articulo
            {
                Codigo = codigo,
                Descripcion = descripcion,
                Estado = false // Siempre inactivo al crear
            };

            _context.Articulos.Add(nuevo);
            await _context.SaveChangesAsync();

            ViewBag.MensajeExito = "Artículo creado correctamente.";
            return await CargarListaView("Index", top, busqueda);
        }

        private async Task<IActionResult> CargarListaView(string viewName, int? top, string busqueda)
        {
            int cantidad = top ?? 6;

            var query = _context.Articulos.AsQueryable();

            if (!string.IsNullOrEmpty(busqueda))
            {
                string buscar = busqueda.ToLower();
                query = query.Where(a =>
                    a.Codigo.ToLower().Contains(buscar) ||
                    a.Descripcion.ToLower().Contains(buscar)
                );
            }

            var lista = await query
                .OrderBy(a => a.Descripcion)
                .Take(cantidad)
                .ToListAsync();

            ViewBag.Top = cantidad;
            ViewBag.Busqueda = busqueda;

            return View(viewName, lista);
        }
    }
}
