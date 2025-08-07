using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Monetix.Data;
using Monetix.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace MonetixBeer.Controllers
{
    public class GastosController : Controller
    {
        private readonly InventarioDbContext _context;

        public GastosController(InventarioDbContext context)
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
        public async Task<IActionResult> Create(string descripcionGasto, int cantidad, decimal montoGastado, DateTime fechaGasto, int? top, string busqueda)
        {
            if (!UsuarioAutenticado())
                return RedirectToAction("Login", "Auth");

            var nuevoGasto = new Gasto
            {
                DescripcionGasto = descripcionGasto,
                Cantidad = cantidad,
                MontoGastado = montoGastado,
                FechaGasto = fechaGasto
            };

            _context.Gastos.Add(nuevoGasto);
            await _context.SaveChangesAsync();

            ViewBag.MensajeExito = "Gasto registrado correctamente.";
            return await CargarListaView("Index", top, busqueda);
        }

        private async Task<IActionResult> CargarListaView(string viewName, int? top, string busqueda)
        {
            int cantidad = top ?? 10;
            var query = _context.Gastos.AsQueryable();

            if (!string.IsNullOrEmpty(busqueda))
            {
                string buscar = busqueda.ToLower();
                query = query.Where(g =>
                    g.DescripcionGasto.ToLower().Contains(buscar)
                );
            }

            var lista = await query
                .OrderByDescending(g => g.FechaGasto)
                .Take(cantidad)
                .ToListAsync();

            ViewBag.Top = cantidad;
            ViewBag.Busqueda = busqueda;

            // Cargar balance por periodo
            var balancesVista = await _context.VistaBalanceArticulos.ToListAsync(); // Vista_BalanceArticulos
            var gastosAgrupados = await _context.Gastos
                .Where(g => g.FechaGasto != null)
                .GroupBy(g => g.FechaGasto.Value.Year * 100 + g.FechaGasto.Value.Month)
                .Select(g => new
                {
                    Periodo = g.Key,
                    TotalGastos = g.Sum(x => x.MontoGastado ?? 0)
                })
                .ToListAsync();

            var balancePorPeriodo = balancesVista
                .GroupBy(b => b.Periodo)
                .Select(b => new BalancePorPeriodo
                {
                    Periodo = b.Key,
                    Balance = b.Sum(x => x.GananciaBruta)
                }).ToList();

            foreach (var item in balancePorPeriodo)
            {
                var gasto = gastosAgrupados.FirstOrDefault(g => g.Periodo == item.Periodo);
                if (gasto != null)
                    item.Balance -= gasto.TotalGastos;
            }

            ViewBag.BalancesPorPeriodo = balancePorPeriodo;

            return View(viewName, lista);
        }
    }
}
