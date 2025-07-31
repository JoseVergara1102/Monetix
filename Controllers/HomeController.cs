using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monetix.Models;

namespace Monetix.Controllers
{
    [Authorize] // Requiere autenticaci�n para acceder a cualquier acci�n
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Obtener nombre del usuario desde la sesi�n (opcional)
            ViewBag.Usuario = HttpContext.Session.GetString("usuario") ?? "Invitado";

            return View(); // Vista protegida
        }

        public IActionResult Privacy()
        {
            return View(); // Vista protegida tambi�n
        }

        [AllowAnonymous] // Permite acceso sin autenticaci�n
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
