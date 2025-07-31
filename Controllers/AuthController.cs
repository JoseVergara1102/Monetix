using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Monetix.Data;
using Monetix.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Monetix.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ========== LOGIN (GET) ==========
        [HttpGet]
        public IActionResult Login()
        {
            CargarRolesActivos();

            if (TempData["RegistroExitoso"] != null)
                ViewBag.MensajeExito = TempData["RegistroExitoso"];

            return View();
        }

        // ========== LOGIN (POST) ==========
        [HttpPost]
        public async Task<IActionResult> Login(string usuario, string pass)
        {
            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(pass))
            {
                ViewBag.Error = "Debe ingresar usuario y contraseña.";
                CargarRolesActivos();
                return View();
            }

            var usuarioEncontrado = _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefault(u => u.UsuarioNombre == usuario);

            if (usuarioEncontrado == null)
            {
                ViewBag.Error = "Credenciales inválidas.";
                CargarRolesActivos();
                return View();
            }

            if (!usuarioEncontrado.Estado)
            {
                ViewBag.Error = "Usuario inactivo.";
                CargarRolesActivos();
                return View();
            }

            if (!BCrypt.Net.BCrypt.Verify(pass, usuarioEncontrado.Contrasena))
            {
                ViewBag.Error = "Credenciales inválidas.";
                CargarRolesActivos();
                return View();
            }

            // Guardar en sesión
            HttpContext.Session.SetInt32("idUsuario", usuarioEncontrado.IdUsuario);
            HttpContext.Session.SetInt32("idRol", usuarioEncontrado.IdRol);
            HttpContext.Session.SetString("usuario", usuarioEncontrado.UsuarioNombre);

            // Crear claims
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, usuarioEncontrado.UsuarioNombre),
        new Claim(ClaimTypes.Role, usuarioEncontrado.Rol.Nombre),
        new Claim("idUsuario", usuarioEncontrado.IdUsuario.ToString()),
        new Claim("idRol", usuarioEncontrado.IdRol.ToString())
    };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = false,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30),
                AllowRefresh = true
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

            return RedirectToAction("Index", "Home");
        }

        // ========== REGISTRO ==========
        [HttpPost]
        public IActionResult Register(string UsuarioNombre, string Contrasena, int idRol)
        {
            if (string.IsNullOrWhiteSpace(UsuarioNombre) || string.IsNullOrWhiteSpace(Contrasena) || idRol == 0)
            {
                ViewBag.Error = "Todos los campos son obligatorios.";
                CargarRolesActivos();
                return View("Login");
            }

            bool existe = _context.Usuarios.Any(u => u.UsuarioNombre == UsuarioNombre);
            if (existe)
            {
                ViewBag.Error = "El nombre de usuario ya existe.";
                CargarRolesActivos();
                return View("Login");
            }

            var nuevoUsuario = new Usuario
            {
                UsuarioNombre = UsuarioNombre,
                Contrasena = Hashear(Contrasena),
                IdRol = idRol,
                Estado = true
            };

            _context.Usuarios.Add(nuevoUsuario);
            _context.SaveChanges();

            TempData["RegistroExitoso"] = "Usuario registrado exitosamente.";
            return RedirectToAction("Login");
        }

        // ========== LOGOUT ==========
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // ========== HASH ==========
        private string Hashear(string textoPlano)
        {
            return BCrypt.Net.BCrypt.HashPassword(textoPlano);
        }

        // ========== CARGAR ROLES ==========
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
