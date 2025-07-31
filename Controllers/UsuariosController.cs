using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Monetix.Data;
using Monetix.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Monetix.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsuariosController(ApplicationDbContext context)
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
        public async Task<IActionResult> Edit(int idUsuario, string usuarioNombre, int idRol, int? top, string busqueda)
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Auth");

            var usuario = await _context.Usuarios.FindAsync(idUsuario);
            if (usuario == null)
            {
                ViewBag.MensajeError = "Usuario no encontrado.";
                return await CargarListaView("Index", top, busqueda);
            }

            // Validar nombre de usuario único
            bool nombreExiste = await _context.Usuarios.AnyAsync(u =>
                u.IdUsuario != idUsuario && u.UsuarioNombre == usuarioNombre);
            if (nombreExiste)
            {
                ViewBag.MensajeError = "Ya existe un usuario con ese nombre.";
                return await CargarListaView("Index", top, busqueda);
            }

            usuario.UsuarioNombre = usuarioNombre;
            usuario.IdRol = idRol;

            await _context.SaveChangesAsync();
            ViewBag.MensajeExito = "Usuario actualizado correctamente.";
            return await CargarListaView("Index", top, busqueda);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(int idUsuario, string nuevaContrasena, int? top, string busqueda)
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Auth");

            if (string.IsNullOrWhiteSpace(nuevaContrasena))
            {
                ViewBag.MensajeError = "La nueva contraseña no puede estar vacía.";
                return await CargarListaView("Index", top, busqueda);
            }

            var usuario = await _context.Usuarios.FindAsync(idUsuario);
            if (usuario == null)
            {
                ViewBag.MensajeError = "Usuario no encontrado.";
                return await CargarListaView("Index", top, busqueda);
            }

            usuario.Contrasena = BCrypt.Net.BCrypt.HashPassword(nuevaContrasena);
            await _context.SaveChangesAsync();

            ViewBag.MensajeExito = "Contraseña restablecida correctamente.";
            return await CargarListaView("Index", top, busqueda);
        }

        [HttpPost]
        public async Task<IActionResult> Activar(int id, int? top, string busqueda)
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Auth");

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                ViewBag.MensajeError = "Usuario no encontrado.";
                return await CargarListaView("Index", top, busqueda);
            }

            usuario.Estado = true;
            await _context.SaveChangesAsync();

            ViewBag.MensajeExito = "Usuario activado correctamente.";
            return await CargarListaView("Index", top, busqueda);
        }

        [HttpPost]
        public async Task<IActionResult> Desactivar(int id, int? top, string busqueda)
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Auth");

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                ViewBag.MensajeError = "Usuario no encontrado.";
                return await CargarListaView("Index", top, busqueda);
            }

            usuario.Estado = false;
            await _context.SaveChangesAsync();

            ViewBag.MensajeExito = "Usuario desactivado correctamente.";
            return await CargarListaView("Index", top, busqueda);
        }

        private async Task<IActionResult> CargarListaView(string viewName, int? top, string busqueda)
        {
            int cantidad = top ?? 10;

            var query = _context.Usuarios
                .Include(u => u.Rol)
                .AsQueryable();

            if (!string.IsNullOrEmpty(busqueda))
            {
                query = query.Where(u => u.UsuarioNombre.Contains(busqueda));
            }

            var lista = await query
                .OrderBy(u => u.UsuarioNombre)
                .Take(cantidad)
                .ToListAsync();

            ViewBag.Top = cantidad;
            ViewBag.Busqueda = busqueda;

            CargarRolesActivos();

            return View(viewName, lista);
        }

        private void CargarRolesActivos()
        {
            var roles = _context.Roles
                .Where(r => r.Estado == true)
                .OrderBy(r => r.Nombre)
                .ToList();

            ViewBag.Roles = roles;
        }
    }
}
