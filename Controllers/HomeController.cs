using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monetix.Models;

namespace Monetix.Controllers
{
    [Authorize] // Requiere autenticación para acceder a cualquier acción
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Obtener nombre del usuario desde la sesión (opcional)
            ViewBag.Usuario = HttpContext.Session.GetString("usuario") ?? "Invitado";

            return View(); // Vista protegida
        }

        public IActionResult Privacy()
        {
            return View(); // Vista protegida también
        }

        [AllowAnonymous] // Permite acceso sin autenticación
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
