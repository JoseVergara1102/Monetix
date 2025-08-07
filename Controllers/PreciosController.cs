using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Monetix.Data;
using Monetix.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Monetix.Controllers
{
    public class PreciosController : Controller
    {
        private readonly InventarioDbContext _context;

        public PreciosController(InventarioDbContext context)
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

            var query = _context.Precios
                .Include(p => p.Articulo)
                .OrderByDescending(p => p.IdPrecio)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                string b = busqueda.ToLower();
                query = query.Where(p =>
                    p.Articulo.Descripcion.ToLower().Contains(b) ||
                    p.Articulo.Codigo.ToLower().Contains(b));
            }

            ViewBag.Top = cantidad;
            ViewBag.Busqueda = busqueda;
            ViewBag.Articulos = await _context.Articulos
                .Where(a => a.Estado == true)
                .OrderBy(a => a.Descripcion)
                .ToListAsync();

            return View(await query.Take(cantidad).ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind(Prefix = "")] Precio precio, int? top, string busqueda)
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Auth");

            ModelState.Remove("busqueda");
            ModelState.Remove("top");

            if (ModelState.IsValid)
            {
                precio.Estado = true;
                _context.Precios.Add(precio);
                await _context.SaveChangesAsync();
                ViewBag.MensajeExito = "Precio registrado correctamente.";
            }
            else
            {
                ViewBag.MensajeError = "Error al registrar el precio. Verifique los datos.";
                ViewBag.AbrirModal = "true";
                ViewBag.PrecioFallido = precio;
            }

            return await RecargarLista(top, busqueda);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Precio precio, int? top, string busqueda)
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Auth");

            ModelState.Remove("busqueda");
            ModelState.Remove("top");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Precios.Update(precio);
                    await _context.SaveChangesAsync();
                    ViewBag.MensajeExito = "Precio actualizado correctamente.";
                }
                catch (Exception)
                {
                    ViewBag.MensajeError = "Error al actualizar el precio.";
                }
            }
            else
            {
                ViewBag.MensajeError = "Error en los datos del precio.";
                ViewBag.AbrirModalEditar = "true";
                ViewBag.PrecioFallido = precio;
            }

            return await RecargarLista(top, busqueda);
        }

        [HttpPost]
        public async Task<IActionResult> CambiarEstado(int id, int? top, string busqueda)
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Auth");

            var precio = await _context.Precios.FindAsync(id);
            if (precio == null)
            {
                ViewBag.MensajeError = "No se encontró el precio a actualizar.";
                return await RecargarLista(top, busqueda);
            }

            precio.Estado = !(precio.Estado ?? true);
            _context.Precios.Update(precio);
            await _context.SaveChangesAsync();

            ViewBag.MensajeExito = $"El precio ha sido {(precio.Estado == true ? "activado" : "desactivado")} correctamente.";

            return await RecargarLista(top, busqueda);
        }

        private async Task<IActionResult> RecargarLista(int? top, string busqueda)
        {
            int cantidad = top ?? 10;

            var query = _context.Precios
                .Include(p => p.Articulo)
                .OrderByDescending(p => p.IdPrecio)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                string b = busqueda.ToLower();
                query = query.Where(p =>
                    p.Articulo.Descripcion.ToLower().Contains(b) ||
                    p.Articulo.Codigo.ToLower().Contains(b));
            }

            ViewBag.Top = cantidad;
            ViewBag.Busqueda = busqueda;
            ViewBag.Articulos = await _context.Articulos
                .Where(a => a.Estado == true)
                .OrderBy(a => a.Descripcion)
                .ToListAsync();

            return View("Index", await query.Take(cantidad).ToListAsync());
        }
    }
}
