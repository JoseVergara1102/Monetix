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
    public class SalidasController : Controller
    {
        private readonly InventarioDbContext _context;

        public SalidasController(InventarioDbContext context)
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
        public async Task<IActionResult> Create(Salida salida, int? top, string busqueda)
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Auth");

            ModelState.Remove("busqueda");

            try
            {
                if (ModelState.IsValid)
                {
                    // Validar existencia del artículo
                    var articulo = await _context.Articulos.FirstOrDefaultAsync(a => a.IdArticulo == salida.IdArticulo);
                    if (articulo == null)
                    {
                        ViewBag.MensajeError = "El artículo seleccionado no existe.";
                        ViewBag.AbrirModal = "true";
                        return await CargarListaView("Index", top, busqueda);
                    }

                    // Validar existencia del cliente
                    var cliente = await _context.ClientesBeer.FirstOrDefaultAsync(c => c.IdCliente == salida.IdCliente);
                    if (cliente == null)
                    {
                        ViewBag.MensajeError = "El cliente seleccionado no existe.";
                        ViewBag.AbrirModal = "true";
                        return await CargarListaView("Index", top, busqueda);
                    }

                    // Buscar el precio actual del artículo
                    var precio = await _context.Precios
                        .Where(p => p.IdArticulo == salida.IdArticulo && p.Medida == salida.Medida && p.Estado == true)
                        .OrderByDescending(p => p.IdPrecio)
                        .FirstOrDefaultAsync();

                    if (precio == null)
                    {
                        ViewBag.MensajeError = "No se encontró un precio asignado al artículo.";
                        ViewBag.AbrirModal = "true";
                        return await CargarListaView("Index", top, busqueda);
                    }

                    // Calcular cantidad de unidades de la salida
                    if (salida.Medida == "PorCajas")
                    {
                        salida.CantidadUnidades = salida.Cantidad * 38;
                        salida.MontoAPagar = precio.PrecioValor * salida.Cantidad;
                    }
                    else if (salida.Medida == "Unidad")
                    {
                        salida.CantidadUnidades = salida.Cantidad;
                        salida.MontoAPagar = precio.PrecioValor * salida.Cantidad;
                    }
                    else
                    {
                        salida.CantidadUnidades = salida.Cantidad;
                        salida.MontoAPagar = precio.PrecioValor * salida.Cantidad;
                    }

                    // Validar disponibilidad en Entradas activas
                    var entradasActivas = await _context.Entradas
                        .Where(e => e.IdArticulo == salida.IdArticulo && e.Estado == true)
                        .ToListAsync();

                    int totalUnidadesEntrada = entradasActivas.Sum(e =>
                        e.Medida == "PorCajas" ? (e.Cantidad ?? 0) * 38 :
                        e.Medida == "Unidad" ? (e.Cantidad ?? 0) : 0);

                    int totalUnidadesSalidas = await _context.Salidas
                        .Where(s => s.IdArticulo == salida.IdArticulo && s.Estado == true)
                        .SumAsync(s => (int)(s.CantidadUnidades ?? 0));

                    int unidadesDisponibles = totalUnidadesEntrada - totalUnidadesSalidas;

                    if (salida.CantidadUnidades > unidadesDisponibles)
                    {
                        ViewBag.MensajeError = $"No hay suficiente stock. Disponibles: {unidadesDisponibles} unidades.";
                        ViewBag.AbrirModal = "true";
                        ViewBag.SalidaFallida = salida;
                        return await CargarListaView("Index", top, busqueda);
                    }

                    // Registrar salida
                    salida.FechaSalida = DateTime.Now;
                    salida.Estado = false;

                    _context.Salidas.Add(salida);
                    await _context.SaveChangesAsync();

                    // 🔍 Verificar si ya no queda stock
                    entradasActivas = await _context.Entradas
                        .Where(e => e.IdArticulo == salida.IdArticulo && e.Estado == true)
                        .ToListAsync();

                    totalUnidadesEntrada = entradasActivas.Sum(e =>
                        e.Medida == "PorCajas" ? (e.Cantidad ?? 0) * 38 :
                        e.Medida == "Unidad" ? (e.Cantidad ?? 0) : 0);

                    totalUnidadesSalidas = await _context.Salidas
                        .Where(s => s.IdArticulo == salida.IdArticulo && s.Estado == true)
                        .SumAsync(s => (int)(s.CantidadUnidades ?? 0));

                    if (totalUnidadesSalidas >= totalUnidadesEntrada)
                    {
                        foreach (var entrada in entradasActivas)
                        {
                            entrada.Estado = false;
                        }
                        await _context.SaveChangesAsync();
                    }

                    ViewBag.MensajeExito = "Salida registrada correctamente.";
                }
                else
                {
                    var errores = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    ViewBag.MensajeError = "Error al registrar la salida: " + string.Join("; ", errores);
                    ViewBag.AbrirModal = "true";
                    ViewBag.SalidaFallida = salida;
                }
            }
            catch (Exception ex)
            {
                ViewBag.MensajeError = "Excepción: " + ex.Message;
                ViewBag.AbrirModal = "true";
                ViewBag.SalidaFallida = salida;
            }

            return await CargarListaView("Index", top, busqueda);
        }

        private async Task<IActionResult> CargarListaView(string viewName, int? top, string busqueda)
        {
            int cantidad = top ?? 10;

            var query = _context.Salidas
                .Include(s => s.Articulo)
                .Include(s => s.Cliente)
                .OrderByDescending(s => s.FechaSalida)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                string b = busqueda.ToLower();

                query = query.Where(s =>
                    (s.Articulo.Descripcion != null && s.Articulo.Descripcion.ToLower().Contains(b)) ||
                    (s.Articulo.Codigo != null && s.Articulo.Codigo.ToLower().Contains(b)) ||
                    (((s.Cliente.PrimerNom ?? "") + " " +
                      (s.Cliente.SegundoNom ?? "") + " " +
                      (s.Cliente.PrimerApe ?? "") + " " +
                      (s.Cliente.SegundoApe ?? "")).ToLower().Contains(b))
                );
            }

            var salidas = await query.Take(cantidad).ToListAsync();

            ViewBag.Top = cantidad;
            ViewBag.Busqueda = busqueda;
            ViewBag.Articulos = await _context.Articulos
                .Where(a => a.Estado == true)
                .OrderBy(a => a.Descripcion)
                .ToListAsync();

            ViewBag.Clientes = await _context.ClientesBeer
                .OrderBy(c => c.PrimerNom)
                .ToListAsync();

            return View(viewName, salidas);
        }

        [HttpPost]
        public async Task<IActionResult> PagarSalida(int id)
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Auth");

            var salida = await _context.Salidas.FindAsync(id);
            if (salida == null)
                return NotFound();

            // Crear el pago SIN NumRecibo aún
            var pago = new PagoBeer
            {
                IdSalida = salida.IdSalida,
                CantidadPagada = salida.MontoAPagar ?? 0,
                CantidadDebe = 0,
                Estado = true
            };

            // Agregar a contexto
            _context.PagosBeer.Add(pago);
            await _context.SaveChangesAsync(); // Guarda y genera IdPago

            // Asignar NumRecibo = IdPago y actualizar
            pago.NumRecibo = pago.IdPago;
            _context.PagosBeer.Update(pago);

            // Marcar salida como pagada
            salida.Estado = true;
            _context.Salidas.Update(salida);

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Salidas");
        }

        // GET: Salidas/AddOne/5
        public async Task<IActionResult> AddOne(int id)
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Auth");

            var salida = await _context.Salidas
                .Include(s => s.Articulo)
                .Include(s => s.Cliente)
                .FirstOrDefaultAsync(s => s.IdSalida == id);

            if (salida == null || salida.Estado == false)
                return NotFound();

            return View(salida);
        }

        // POST: Salidas/AddOne/5
        [HttpPost, ActionName("AddOne")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOneConfirmed(int id)
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Auth");

            var salida = await _context.Salidas.FindAsync(id);
            if (salida == null || salida.Estado == false)
            {
                TempData["Error"] = "No se encontró la salida especificada o ya está cerrada.";
                return RedirectToAction(nameof(Index));
            }

            // Obtener precio actualizado
            var precio = await _context.Precios
                .Where(p => p.IdArticulo == salida.IdArticulo && p.Medida == salida.Medida && p.Estado == true)
                .OrderByDescending(p => p.IdPrecio)
                .FirstOrDefaultAsync();

            if (precio == null)
            {
                TempData["Error"] = "No se encontró un precio activo para este artículo.";
                return RedirectToAction(nameof(Index));
            }

            // Verificar stock
            var entradasActivas = await _context.Entradas
                .Where(e => e.IdArticulo == salida.IdArticulo && e.Estado == true)
                .ToListAsync();

            int totalUnidadesEntrada = entradasActivas.Sum(e =>
                e.Medida == "PorCajas" ? (e.Cantidad ?? 0) * 38 :
                e.Medida == "Unidad" ? (e.Cantidad ?? 0) : 0);

            int totalUnidadesSalidas = await _context.Salidas
                .Where(s => s.IdArticulo == salida.IdArticulo && s.Estado == true)
                .SumAsync(s => (int)(s.CantidadUnidades ?? 0));

            int unidadesDisponibles = totalUnidadesEntrada - totalUnidadesSalidas;
            int unidadesASumar = salida.Medida == "PorCajas" ? 38 : 1;

            if (unidadesASumar > unidadesDisponibles)
            {
                TempData["Error"] = $"No hay suficiente stock para añadir +1. Disponibles: {unidadesDisponibles} unidades.";
                return RedirectToAction(nameof(Index));
            }

            // Aumentar valores
            salida.Cantidad += 1;
            salida.CantidadUnidades += unidadesASumar;

            if (salida.Medida == "PorCajas")
            {
                salida.MontoAPagar = precio.PrecioValor * salida.Cantidad;
            }
            else if (salida.Medida == "Unidad")
            {
                salida.MontoAPagar = precio.PrecioValor * salida.Cantidad;
            }
            else
            {
                salida.MontoAPagar = precio.PrecioValor * salida.Cantidad; // Fallback
            }

            _context.Salidas.Update(salida);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Se ha añadido correctamente una unidad a la salida.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Abonar(int IdSalida, decimal CantidadAbono)
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Auth");

            var salida = await _context.Salidas.FirstOrDefaultAsync(s => s.IdSalida == IdSalida);
            if (salida == null)
            {
                TempData["Error"] = "No se encontró la salida especificada.";
                return RedirectToAction("Index");
            }

            // Sumar todos los abonos anteriores
            var totalAbonado = await _context.PagosBeer
                .Where(p => p.IdSalida == IdSalida)
                .SumAsync(p => (decimal?)p.CantidadPagada) ?? 0;

            var nuevoTotal = totalAbonado + CantidadAbono;
            var debe = (salida.MontoAPagar ?? 0) - nuevoTotal;
            if (debe < 0)
            {
                TempData["Error"] = "El abono no puede ser mayor al monto a pagar.";
                return RedirectToAction("Index");
            }
            var nuevoPago = new PagoBeer
            {
                IdSalida = IdSalida,
                CantidadPagada = CantidadAbono,
                CantidadDebe = debe,
                Estado = false
            };

            _context.PagosBeer.Add(nuevoPago);
            await _context.SaveChangesAsync();

            // Asignar número de recibo (después del save para tener IdPago)
            nuevoPago.NumRecibo = nuevoPago.IdPago;
            _context.PagosBeer.Update(nuevoPago);

            // Verificar si ya se completó el pago
            if (nuevoTotal >= (salida.MontoAPagar ?? 0))
            {
                salida.Estado = true;

                // Marcar todos los pagos como pagados
                var pagosRelacionados = await _context.PagosBeer
                    .Where(p => p.IdSalida == IdSalida)
                    .ToListAsync();

                foreach (var p in pagosRelacionados)
                    p.Estado = true;

                _context.Salidas.Update(salida);
                _context.PagosBeer.UpdateRange(pagosRelacionados);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Abono registrado correctamente.";
            return RedirectToAction("Index");
        }

    }
}
