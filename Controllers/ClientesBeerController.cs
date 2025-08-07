using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Monetix.Models;
using Monetix.Data;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;

namespace Monetix.Controllers
{
    public class ClientesBeerController : Controller
    {
        private readonly InventarioDbContext _context;

        public ClientesBeerController(InventarioDbContext context)
        {
            _context = context;
        }

        private bool UsuarioAutenticado()
        {
            return HttpContext.Session.GetInt32("idUsuario") != null;
        }

        public async Task<IActionResult> ClientesBeer(string busqueda, int? top)
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Auth");

            return await CargarListaView("Clientes/ClientesBeer", top, busqueda); // <- Ruta corregida
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            string primerNom, string segundoNom, string primerApe, string segundoApe,
            string direccion, int idBarrio, int? top, string busqueda)
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Auth");

            bool existe = await _context.ClientesBeer.AnyAsync(c =>
                c.PrimerNom == primerNom &&
                c.SegundoNom == segundoNom &&
                c.PrimerApe == primerApe &&
                c.SegundoApe == segundoApe);

            if (existe)
            {
                ViewBag.MensajeError = "Ya existe un cliente con los mismos nombres y apellidos.";
                ViewBag.AbrirModal = "true";
                return await CargarListaView("Clientes/ClientesBeer", top, busqueda);
            }

            var cliente = new ClienteBeer
            {
                PrimerNom = primerNom,
                SegundoNom = segundoNom,
                PrimerApe = primerApe,
                SegundoApe = segundoApe,
                Direccion = direccion,
                IdBarrio = idBarrio,
                Estado = true
            };

            _context.ClientesBeer.Add(cliente);
            await _context.SaveChangesAsync();

            ViewBag.MensajeExito = "Cliente creado correctamente.";
            return await CargarListaView("Clientes/ClientesBeer", top, busqueda);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(
            int idCliente, string primerNom, string segundoNom, string primerApe, string segundoApe,
            string direccion, int idBarrio, int? top, string busqueda)
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Auth");

            var cliente = await _context.ClientesBeer.FindAsync(idCliente);
            if (cliente == null)
            {
                ViewBag.MensajeError = "Cliente no encontrado.";
                return await CargarListaView("Clientes/ClientesBeer", top, busqueda);
            }

            bool existe = await _context.ClientesBeer.AnyAsync(c =>
                c.IdCliente != idCliente &&
                c.PrimerNom == primerNom &&
                c.SegundoNom == segundoNom &&
                c.PrimerApe == primerApe &&
                c.SegundoApe == segundoApe);

            if (existe)
            {
                ViewBag.MensajeError = "Otro cliente con los mismos nombres y apellidos ya existe.";
                return await CargarListaView("Clientes/ClientesBeer", top, busqueda);
            }

            cliente.PrimerNom = primerNom;
            cliente.SegundoNom = segundoNom;
            cliente.PrimerApe = primerApe;
            cliente.SegundoApe = segundoApe;
            cliente.Direccion = direccion;
            cliente.IdBarrio = idBarrio;

            await _context.SaveChangesAsync();

            ViewBag.MensajeExito = "Cliente actualizado correctamente.";
            return await CargarListaView("Clientes/ClientesBeer", top, busqueda);
        }

        [HttpPost]
        public async Task<IActionResult> Activar(int id, int? top, string busqueda)
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Auth");

            var cliente = await _context.ClientesBeer.FindAsync(id);
            if (cliente == null)
            {
                ViewBag.MensajeError = "Cliente no encontrado.";
                return await CargarListaView("Clientes/ClientesBeer", top, busqueda);
            }

            cliente.Estado = true;
            await _context.SaveChangesAsync();

            ViewBag.MensajeExito = "Cliente activado correctamente.";
            return await CargarListaView("Clientes/ClientesBeer", top, busqueda);
        }

        [HttpPost]
        public async Task<IActionResult> Desactivar(int id, int? top, string busqueda)
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Auth");

            var cliente = await _context.ClientesBeer.FindAsync(id);
            if (cliente == null)
            {
                ViewBag.MensajeError = "Cliente no encontrado.";
                return await CargarListaView("Clientes/ClientesBeer", top, busqueda);
            }

            cliente.Estado = false;
            await _context.SaveChangesAsync();

            ViewBag.MensajeExito = "Cliente desactivado correctamente.";
            return await CargarListaView("Clientes/ClientesBeer", top, busqueda);
        }

        private async Task<IActionResult> CargarListaView(string viewName, int? top, string busqueda)
        {
            int cantidad = top ?? 10;

            var query = _context.ClientesBeer
                .Include(c => c.Barrio)
                .AsQueryable();

            if (!string.IsNullOrEmpty(busqueda))
            {
                query = query.Where(c =>
                    (c.PrimerNom + " " + c.SegundoNom + " " + c.PrimerApe + " " + c.SegundoApe).Contains(busqueda));
            }

            var lista = await query
                .OrderBy(c => c.PrimerNom)
                .Take(cantidad)
                .ToListAsync();

            var barriosActivos = await _context.Barrios
                .Where(b => b.Estado == true)
                .OrderBy(b => b.Nombre)
                .ToListAsync();

            ViewBag.Top = cantidad;
            ViewBag.Busqueda = busqueda;
            ViewBag.Barrios = barriosActivos;

            return View("~/Views/Clientes/ClientesBeer.cshtml", lista);
        }
    }
}
