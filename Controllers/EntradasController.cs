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
    public class EntradasController : Controller
    {
        private readonly InventarioDbContext _context;

        public EntradasController(InventarioDbContext context)
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
        public async Task<IActionResult> Create(Entrada entrada, int? top, string busqueda)
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Auth");

            // Excluir 'busqueda' de la validación del modelo
            ModelState.Remove("busqueda");

            try
            {
                if (ModelState.IsValid)
                {
                    entrada.FechaEntrada = DateTime.Now;

                    if (entrada.Medida == "Unidad")
                        entrada.CantidadUnidades = entrada.Cantidad;
                    else if (entrada.Medida == "PorCajas")
                        entrada.CantidadUnidades = entrada.Cantidad * 38;

                    entrada.Estado = true;

                    _context.Entradas.Add(entrada);
                    await _context.SaveChangesAsync();

                    ViewBag.MensajeExito = "Entrada registrada correctamente.";
                }
                else
                {
                    var errores = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    ViewBag.MensajeError = "Error al registrar la entrada: " + string.Join("; ", errores);
                    ViewBag.AbrirModal = "true";
                    ViewBag.EntradaFallida = entrada;
                }
            }
            catch (Exception ex)
            {
                ViewBag.MensajeError = "Excepción: " + ex.Message;
                ViewBag.AbrirModal = "true";
                ViewBag.EntradaFallida = entrada;
            }

            return await CargarListaView("Index", top, busqueda);
        }

        private async Task<IActionResult> CargarListaView(string viewName, int? top, string busqueda)
        {
            int cantidad = top ?? 10;

            var query = _context.Entradas
                .Include(e => e.Articulo)
                .OrderByDescending(e => e.FechaEntrada)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                string b = busqueda.ToLower();
                query = query.Where(e =>
                    e.Articulo.Descripcion.ToLower().Contains(b) ||
                    e.Articulo.Codigo.ToLower().Contains(b));
            }

            var entradas = await query.Take(cantidad).ToListAsync();

            ViewBag.Top = cantidad;
            ViewBag.Busqueda = busqueda;
            ViewBag.Articulos = await _context.Articulos
                .Where(a => a.Estado == true)
                .OrderBy(a => a.Descripcion)
                .ToListAsync();

            return View(viewName, entradas);
        }

        // Método para actualizar el estado de las entradas basado en la vista Vista_Existencias
        public async Task<IActionResult> ActualizarEstadoEntradas()
        {
            // Obtener todas las entradas activas
            var entradas = await _context.Entradas
                .Include(e => e.Articulo)
                .Where(e => e.Estado == true)
                .ToListAsync();

            // Obtener las existencias actuales desde la vista
            var existenciasVista = await _context.VistaExistencias.ToListAsync();

            foreach (var entrada in entradas)
            {
                var existencia = existenciasVista
                    .FirstOrDefault(v => v.IdArticulo == entrada.IdArticulo);

                if (existencia != null)
                {
                    // Si la existencia actual es menor o igual a la cantidad registrada en esta entrada,
                    // entonces se considera que esta entrada ya fue completamente usada
                    if (existencia.CantidadUnidadesDisponible <= entrada.CantidadUnidades)
                    {
                        entrada.Estado = false;
                        _context.Entradas.Update(entrada);
                    }
                }
            }

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "Estados de entradas actualizados correctamente.";
            return RedirectToAction("Index");
        }
    }
}
