using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Monetix.Data;
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace MonetixBeer.Controllers
{
    public class PagosBeerController : Controller
    {
        private readonly InventarioDbContext _context;

        public PagosBeerController(InventarioDbContext context)
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

        private async Task<IActionResult> CargarListaView(string viewName, int? top, string busqueda)
        {
            int cantidad = top ?? 10;

            var query = _context.PagosBeer
                .Include(p => p.Salida)
                    .ThenInclude(s => s.Cliente)
                .AsQueryable();

            if (!string.IsNullOrEmpty(busqueda))
            {
                string buscar = busqueda.ToLower();
                query = query.Where(p =>
                    p.NumRecibo.ToString().ToLower().Contains(buscar) ||
                    p.IdSalida.ToString().Contains(buscar) ||
                    (p.Salida.Cliente.PrimerNom + " " + p.Salida.Cliente.SegundoNom + " " +
                     p.Salida.Cliente.PrimerApe + " " + p.Salida.Cliente.SegundoApe)
                     .ToLower().Contains(buscar)
                );
            }

            var lista = await query
                .OrderByDescending(p => p.IdPago)
                .Take(cantidad)
                .ToListAsync();

            ViewBag.Top = cantidad;
            ViewBag.Busqueda = busqueda;

            return View(viewName, lista); 
        }

        public async Task<IActionResult> Imprimir(int id)
        {
            var pago = await _context.PagosBeer
                .Include(p => p.Salida)
                    .ThenInclude(s => s.Cliente)
                .Include(p => p.Salida)
                    .ThenInclude(s => s.Articulo)
                .FirstOrDefaultAsync(p => p.IdPago == id);

            if (pago == null)
            {
                return NotFound("Pago no encontrado");
            }

            var cliente = pago.Salida.Cliente;
            var articulo = pago.Salida.Articulo;

            string nombreCompleto = $"{cliente.PrimerNom} {cliente.SegundoNom} {cliente.PrimerApe} {cliente.SegundoApe}".Trim();

            string cantidadYUnidad = "";
            if (pago.Salida.Medida == "PorCajas")
            {
                string unidadCaja = pago.Salida.Cantidad == 1 ? "Caja" : "Cajas";
                cantidadYUnidad = $"{pago.Salida.Cantidad} {unidadCaja} de {articulo.Descripcion}";
            }
            else // Unidades
            {
                if (pago.Salida.Cantidad == 1)
                    cantidadYUnidad = $"1 {articulo.Descripcion}";
                else
                    cantidadYUnidad = $"{pago.Salida.Cantidad} {articulo.Descripcion}s";
            }


            using (var ms = new MemoryStream())
            {
                var doc = new PdfDocument();
                var page = doc.AddPage();
                page.Size = PdfSharpCore.PageSize.A5;

                var gfx = XGraphics.FromPdfPage(page);

                var fontTitle = new XFont("Verdana", 18, XFontStyle.Bold);
                var fontLabel = new XFont("Verdana", 12, XFontStyle.Bold);
                var fontText = new XFont("Verdana", 12, XFontStyle.Regular);

                int margin = 40;
                int y = 40;

                // Encabezado centrado
                gfx.DrawString("MONETIXBEER", fontTitle, XBrushes.DarkBlue, new XRect(0, y, page.Width, 30), XStringFormats.TopCenter);
                y += 50;

                // Cliente
                gfx.DrawString("Cliente:", fontLabel, XBrushes.Black, margin, y);
                gfx.DrawString(nombreCompleto, fontText, XBrushes.Black, margin + 100, y);
                y += 25;

                // Número de recibo
                gfx.DrawString("N° Recibo:", fontLabel, XBrushes.Black, margin, y);
                gfx.DrawString(pago.NumRecibo.ToString(), fontText, XBrushes.Black, margin + 100, y);
                y += 25;

                // Fecha de salida
                gfx.DrawString("Fecha Salida:", fontLabel, XBrushes.Black, margin, y);
                gfx.DrawString(
                pago.Salida.FechaSalida?.ToString("yyyy-MM-dd") ?? "Sin fecha",
                fontText,
                XBrushes.Black,
                margin + 100,
                y
                );

                y += 25;

                // Cantidad pagada
                gfx.DrawString("Cantidad Pagada:", fontLabel, XBrushes.Black, margin, y);
                gfx.DrawString($"${pago.CantidadPagada:N2}", fontText, XBrushes.Black, margin + 140, y);
                y += 25;

                // Cantidad por pagar
                gfx.DrawString("Cantidad por Pagar:", fontLabel, XBrushes.Black, margin, y);
                gfx.DrawString($"${pago.CantidadDebe:N2}", fontText, XBrushes.Black, margin + 160, y);
                y += 25;

                // Cantidad y unidad
                gfx.DrawString("Producto Entregado:", fontLabel, XBrushes.Black, margin, y);
                gfx.DrawString(cantidadYUnidad, fontText, XBrushes.Black, margin + 160, y);
                y += 30;

                // Footer
                gfx.DrawLine(XPens.Gray, margin, y, page.Width - margin, y);
                y += 20;
                gfx.DrawString("Gracias por confiar en MonetixBeer.", fontText, XBrushes.Gray, new XRect(0, y, page.Width, 20), XStringFormats.TopCenter);

                doc.Save(ms);
                byte[] pdfBytes = ms.ToArray();

                return File(pdfBytes, "application/pdf", $"Recibo_{pago.NumRecibo}.pdf");
            }

        }

    }
}
