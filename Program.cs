using Microsoft.EntityFrameworkCore;
using Monetix.Data;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Agregar servicios al contenedor
builder.Services.AddControllersWithViews();

// 👉 Agregar IHttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Habilitar sesión
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configurar DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MonetixContabilidad")));

// Configurar autenticación con cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login"; // Redirige si no ha iniciado sesión
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    });

var app = builder.Build();

// Middleware
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ⚠️ Orden correcto
app.UseSession();           // 1. Session
app.UseAuthentication();    // 2. Autenticación
app.UseAuthorization();     // 3. Autorización

// Mapear rutas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
