using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Monetix.Data;
using Monetix.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Monetix.Controllers
{
    public class CuentasBancariasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CuentasBancariasController(ApplicationDbContext context)
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
        public async Task<IActionResult> Create(string numCuenta, string tipoCuenta, int idBanco, int idCliente, int? top, string busqueda)
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Auth");

            // Validación: misma cuenta para el mismo cliente
            bool existe = await _context.CuentasBancarias.AnyAsync(c =>
                c.NumCuenta == numCuenta && c.IdCliente == idCliente
            );

            if (existe)
            {
                ViewBag.MensajeError = "Este cliente ya tiene registrada esta cuenta.";
                ViewBag.AbrirModal = "true";
                return await CargarListaView("Index", top, busqueda);
            }

            var cuenta = new CuentaBancaria
            {
                NumCuenta = numCuenta,
                TipoCuenta = tipoCuenta,
                IdBanco = idBanco,
                IdCliente = idCliente,
                Estado = true
            };

            _context.CuentasBancarias.Add(cuenta);
            await _context.SaveChangesAsync();

            ViewBag.MensajeExito = "Cuenta bancaria creada correctamente.";
            return await CargarListaView("Index", top, busqueda);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int idCuenta, string numCuenta, string tipoCuenta, int idBanco, int idCliente, int? top, string busqueda)
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Auth");

            var cuenta = await _context.CuentasBancarias.FindAsync(idCuenta);
            if (cuenta == null)
            {
                ViewBag.MensajeError = "Cuenta bancaria no encontrada.";
                return await CargarListaView("Index", top, busqueda);
            }

            bool duplicada = await _context.CuentasBancarias.AnyAsync(c =>
                c.IdCuenta != idCuenta &&
                c.NumCuenta == numCuenta &&
                c.IdCliente == idCliente
            );

            if (duplicada)
            {
                ViewBag.MensajeError = "Ya existe otra cuenta con ese número para este cliente.";
                return await CargarListaView("Index", top, busqueda);
            }

            cuenta.NumCuenta = numCuenta;
            cuenta.TipoCuenta = tipoCuenta;
            cuenta.IdBanco = idBanco;
            cuenta.IdCliente = idCliente;

            await _context.SaveChangesAsync();

            ViewBag.MensajeExito = "Cuenta bancaria actualizada correctamente.";
            return await CargarListaView("Index", top, busqueda);
        }

        [HttpPost]
        public async Task<IActionResult> Activar(int id, int? top, string busqueda)
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Auth");

            var cuenta = await _context.CuentasBancarias.FindAsync(id);
            if (cuenta == null)
            {
                ViewBag.MensajeError = "Cuenta bancaria no encontrada.";
                return await CargarListaView("Index", top, busqueda);
            }

            cuenta.Estado = true;
            await _context.SaveChangesAsync();

            ViewBag.MensajeExito = "Cuenta activada correctamente.";
            return await CargarListaView("Index", top, busqueda);
        }

        [HttpPost]
        public async Task<IActionResult> Desactivar(int id, int? top, string busqueda)
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Auth");

            var cuenta = await _context.CuentasBancarias.FindAsync(id);
            if (cuenta == null)
            {
                ViewBag.MensajeError = "Cuenta bancaria no encontrada.";
                return await CargarListaView("Index", top, busqueda);
            }

            cuenta.Estado = false;
            await _context.SaveChangesAsync();

            ViewBag.MensajeExito = "Cuenta desactivada correctamente.";
            return await CargarListaView("Index", top, busqueda);
        }

        private async Task<IActionResult> CargarListaView(string viewName, int? top, string busqueda)
        {
            int cantidad = top ?? 10;

            var query = _context.CuentasBancarias
                .Include(c => c.Banco)
                .Include(c => c.Cliente)
                .AsQueryable();

            if (!string.IsNullOrEmpty(busqueda))
            {
                string buscar = busqueda.ToLower();
                query = query.Where(c =>
                    c.NumCuenta.ToLower().Contains(buscar) ||
                    c.Banco.Nombre.ToLower().Contains(buscar) ||
                    (
                        (c.Cliente.PrimerNom + " " +
                         (c.Cliente.SegundoNom ?? "") + " " +
                         c.Cliente.PrimerApe + " " +
                         (c.Cliente.SegundoApe ?? "")
                        ).ToLower().Contains(buscar)
                    )
                );
            }

            var lista = await query
                .OrderByDescending(c => c.IdCuenta)
                .Take(cantidad)
                .ToListAsync();

            ViewBag.Top = cantidad;
            ViewBag.Busqueda = busqueda;

            ViewBag.TiposCuenta = new List<SelectListItem>
            {
                new SelectListItem { Value = "Cuenta de Ahorros", Text = "Cuenta de Ahorros" },
                new SelectListItem { Value = "Cuenta Corriente", Text = "Cuenta Corriente" }
            };

            ViewBag.Bancos = await _context.Bancos
                .Where(b => b.Estado == true)
                .ToListAsync();

            ViewBag.Clientes = await _context.Clientes
                .Where(c => c.Estado == true)
                .ToListAsync();

            ViewBag.EsPropietario = (Func<int, bool>)(idCuenta =>
            {
                int? idUsuario = HttpContext.Session.GetInt32("idUsuario");
                var cuenta = _context.CuentasBancarias.FirstOrDefault(c => c.IdCuenta == idCuenta);
                return cuenta != null && cuenta.IdCliente == idUsuario;
            });

            return View(viewName, lista);
        }
    }
}
