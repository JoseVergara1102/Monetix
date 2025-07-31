using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Monetix.Data;
using Monetix.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;

namespace Monetix.Controllers
{
    public class BancosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BancosController(ApplicationDbContext context)
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
        public async Task<IActionResult> Create(
            string codigo, string nombre, string codInsFin,
            int? top, string busqueda)
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Auth");

            bool existe = await _context.Bancos.AnyAsync(b =>
                b.Nombre.ToLower() == nombre.ToLower() ||
                b.Codigo.ToLower() == codigo.ToLower()
            );

            if (existe)
            {
                ViewBag.MensajeError = "Ya existe un banco con el mismo nombre o código.";
                ViewBag.AbrirModal = "true";
                return await CargarListaView("Index", top, busqueda);
            }

            var banco = new Banco
            {
                Codigo = codigo,
                Nombre = nombre,
                CodInsFin = codInsFin,
                Estado = true
            };

            _context.Bancos.Add(banco);
            await _context.SaveChangesAsync();

            ViewBag.MensajeExito = "Banco creado correctamente.";
            return await CargarListaView("Index", top, busqueda);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(
            int idBanco, string codigo, string nombre, string codInsFin,
            int? top, string busqueda)
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Auth");

            var banco = await _context.Bancos.FindAsync(idBanco);
            if (banco == null)
            {
                ViewBag.MensajeError = "Banco no encontrado.";
                return await CargarListaView("Index", top, busqueda);
            }

            bool duplicado = await _context.Bancos.AnyAsync(b =>
                b.IdBanco != idBanco &&
                (b.Nombre.ToLower() == nombre.ToLower() ||
                 b.Codigo.ToLower() == codigo.ToLower())
            );

            if (duplicado)
            {
                ViewBag.MensajeError = "Ya existe otro banco con el mismo nombre o código.";
                return await CargarListaView("Index", top, busqueda);
            }

            banco.Codigo = codigo;
            banco.Nombre = nombre;
            banco.CodInsFin = codInsFin;

            await _context.SaveChangesAsync();

            ViewBag.MensajeExito = "Banco actualizado correctamente.";
            return await CargarListaView("Index", top, busqueda);
        }

        [HttpPost]
        public async Task<IActionResult> Activar(int id, int? top, string busqueda)
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Auth");

            var banco = await _context.Bancos.FindAsync(id);
            if (banco == null)
            {
                ViewBag.MensajeError = "Banco no encontrado.";
                return await CargarListaView("Index", top, busqueda);
            }

            banco.Estado = true;
            await _context.SaveChangesAsync();

            ViewBag.MensajeExito = "Banco activado correctamente.";
            return await CargarListaView("Index", top, busqueda);
        }

        [HttpPost]
        public async Task<IActionResult> Desactivar(int id, int? top, string busqueda)
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Auth");

            var banco = await _context.Bancos.FindAsync(id);
            if (banco == null)
            {
                ViewBag.MensajeError = "Banco no encontrado.";
                return await CargarListaView("Index", top, busqueda);
            }

            banco.Estado = false;
            await _context.SaveChangesAsync();

            ViewBag.MensajeExito = "Banco desactivado correctamente.";
            return await CargarListaView("Index", top, busqueda);
        }

        private async Task<IActionResult> CargarListaView(string viewName, int? top, string busqueda)
        {
            int cantidad = top ?? 10;

            var query = _context.Bancos.AsQueryable();

            if (!string.IsNullOrEmpty(busqueda))
            {
                string buscar = busqueda.ToLower();
                query = query.Where(b =>
                    b.Nombre.ToLower().Contains(buscar) ||
                    b.Codigo.ToLower().Contains(buscar) ||
                    b.CodInsFin.ToLower().Contains(buscar)
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
