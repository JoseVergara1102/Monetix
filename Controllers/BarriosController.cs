using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Monetix.Data;
using Monetix.Models;
using System.Linq;
using System.Threading.Tasks;

namespace MonetixBeer.Controllers
{
    public class BarriosController : Controller
    {
        private readonly InventarioDbContext _context;

        public BarriosController(InventarioDbContext context)
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
        public async Task<IActionResult> Create(string nombre, int? top, string busqueda)
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Auth");

            bool existe = await _context.Barrios.AnyAsync(b => b.Nombre.ToLower() == nombre.ToLower());
            if (existe)
            {
                ViewBag.MensajeError = "Ya existe un barrio con ese nombre.";
                ViewBag.AbrirModal = "true";
                return await CargarListaView("Index", top, busqueda);
            }

            _context.Barrios.Add(new Barrio
            {
                Nombre = nombre,
                Estado = true
            });

            await _context.SaveChangesAsync();
            ViewBag.MensajeExito = "Barrio creado correctamente.";
            return await CargarListaView("Index", top, busqueda);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int idBarrio, string nombre, int? top, string busqueda)
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Auth");

            var barrio = await _context.Barrios.FindAsync(idBarrio);
            if (barrio == null)
            {
                ViewBag.MensajeError = "Barrio no encontrado.";
                return await CargarListaView("Index", top, busqueda);
            }

            bool duplicado = await _context.Barrios.AnyAsync(b =>
                b.IdBarrio != idBarrio &&
                b.Nombre.ToLower() == nombre.ToLower()
            );

            if (duplicado)
            {
                ViewBag.MensajeError = "Ya existe otro barrio con ese nombre.";
                return await CargarListaView("Index", top, busqueda);
            }

            barrio.Nombre = nombre;
            await _context.SaveChangesAsync();

            ViewBag.MensajeExito = "Barrio actualizado correctamente.";
            return await CargarListaView("Index", top, busqueda);
        }

        [HttpPost]
        public async Task<IActionResult> Activar(int id, int? top, string busqueda)
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Auth");

            var barrio = await _context.Barrios.FindAsync(id);
            if (barrio == null)
            {
                ViewBag.MensajeError = "Barrio no encontrado.";
                return await CargarListaView("Index", top, busqueda);
            }

            barrio.Estado = true;
            await _context.SaveChangesAsync();

            ViewBag.MensajeExito = "Barrio activado correctamente.";
            return await CargarListaView("Index", top, busqueda);
        }

        [HttpPost]
        public async Task<IActionResult> Desactivar(int id, int? top, string busqueda)
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Auth");

            var barrio = await _context.Barrios.FindAsync(id);
            if (barrio == null)
            {
                ViewBag.MensajeError = "Barrio no encontrado.";
                return await CargarListaView("Index", top, busqueda);
            }

            barrio.Estado = false;
            await _context.SaveChangesAsync();

            ViewBag.MensajeExito = "Barrio desactivado correctamente.";
            return await CargarListaView("Index", top, busqueda);
        }

        private async Task<IActionResult> CargarListaView(string viewName, int? top, string busqueda)
        {
            int cantidad = top ?? 10;

            var query = _context.Barrios.AsQueryable();

            if (!string.IsNullOrEmpty(busqueda))
            {
                string buscar = busqueda.ToLower();
                query = query.Where(b =>
                    b.Nombre.ToLower().Contains(buscar)
                );
            }

            var lista = await query
                .OrderBy(b => b.Nombre)
                .Take(cantidad)
                .ToListAsync();

            ViewBag.Top = cantidad;
            ViewBag.Busqueda = busqueda;

            return View(viewName, lista);
        }
    }
}
