using Microsoft.AspNetCore.Mvc;
using Monetix.Data;
using Monetix.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Monetix.Controllers
{
    public class ClientesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClientesController(ApplicationDbContext context)
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
            string primerNom, string segundoNom, string primerApe, string segundoApe,
            string telefono, string direccion, string barrio, string email,
            int? top, string busqueda)
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Auth");

            bool existe = await _context.Clientes.AnyAsync(c =>
                c.PrimerNom == primerNom &&
                c.SegundoNom == segundoNom &&
                c.PrimerApe == primerApe &&
                c.SegundoApe == segundoApe
            );

            if (existe)
            {
                ViewBag.MensajeError = "Ya existe un cliente con los mismos nombres y apellidos.";
                ViewBag.AbrirModal = "true";
                return await CargarListaView("Index", top, busqueda);
            }

            var cliente = new Cliente
            {
                PrimerNom = primerNom,
                SegundoNom = segundoNom,
                PrimerApe = primerApe,
                SegundoApe = segundoApe,
                Telefono = telefono,
                Direccion = direccion,
                Barrio = barrio,
                Email = email,
                Estado = true,
                FechaRegistro = DateTime.Now
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            ViewBag.MensajeExito = "Cliente creado correctamente.";
            return await CargarListaView("Index", top, busqueda);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(
            int idCliente, string primerNom, string segundoNom, string primerApe, string segundoApe,
            string telefono, string direccion, string barrio, string email,
            int? top, string busqueda)
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Auth");

            var cliente = await _context.Clientes.FindAsync(idCliente);
            if (cliente == null)
            {
                ViewBag.MensajeError = "Cliente no encontrado.";
                return await CargarListaView("Index", top, busqueda);
            }

            bool existe = await _context.Clientes.AnyAsync(c =>
                c.IdCliente != idCliente &&
                c.PrimerNom == primerNom &&
                c.SegundoNom == segundoNom &&
                c.PrimerApe == primerApe &&
                c.SegundoApe == segundoApe
            );

            if (existe)
            {
                ViewBag.MensajeError = "Otro cliente con los mismos nombres y apellidos ya existe.";
                return await CargarListaView("Index", top, busqueda);
            }

            cliente.PrimerNom = primerNom;
            cliente.SegundoNom = segundoNom;
            cliente.PrimerApe = primerApe;
            cliente.SegundoApe = segundoApe;
            cliente.Telefono = telefono;
            cliente.Direccion = direccion;
            cliente.Barrio = barrio;
            cliente.Email = email;

            await _context.SaveChangesAsync();

            ViewBag.MensajeExito = "Cliente actualizado correctamente.";
            return await CargarListaView("Index", top, busqueda);
        }

        [HttpPost]
        public async Task<IActionResult> Activar(int id, int? top, string busqueda)
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Auth");

            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
            {
                ViewBag.MensajeError = "Cliente no encontrado.";
                return await CargarListaView("Index", top, busqueda);
            }

            cliente.Estado = true;
            await _context.SaveChangesAsync();

            ViewBag.MensajeExito = "Cliente activado correctamente.";
            return await CargarListaView("Index", top, busqueda);
        }

        [HttpPost]
        public async Task<IActionResult> Desactivar(int id, int? top, string busqueda)
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Auth");

            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
            {
                ViewBag.MensajeError = "Cliente no encontrado.";
                return await CargarListaView("Index", top, busqueda);
            }

            cliente.Estado = false;
            await _context.SaveChangesAsync();

            ViewBag.MensajeExito = "Cliente desactivado correctamente.";
            return await CargarListaView("Index", top, busqueda);
        }

        private async Task<IActionResult> CargarListaView(string viewName, int? top, string busqueda)
        {
            int cantidad = top ?? 10;

            var query = _context.Clientes.AsQueryable();

            if (!string.IsNullOrEmpty(busqueda))
            {
                query = query.Where(c =>
                    (c.PrimerNom + " " + c.SegundoNom + " " + c.PrimerApe + " " + c.SegundoApe).Contains(busqueda));
            }

            var lista = await query
                .OrderBy(c => c.PrimerNom)
                .Take(cantidad)
                .ToListAsync();

            ViewBag.Top = cantidad;
            ViewBag.Busqueda = busqueda;

            return View(viewName, lista);
        }
    }
}