using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WebPasteleria.Controllers;
using WebPasteleria.Models;

namespace WebPasteleria.Controllers
{
    public class CuentaController : Controller
    {
        private readonly FinalPasteleriaContext _context;

        public CuentaController(FinalPasteleriaContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Por favor, ingresa usuario y contraseña válidos.");
                return View(model);
            }

            // Buscar cliente por email/usuario
            var cliente = await _context.Cliente
                .Where(c => c.Estado == 1 && c.Email == model.usuario)
                .FirstOrDefaultAsync();

            if (cliente != null)
            {
                // Crear claims para el cliente
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, cliente.Id.ToString()),
                    new Claim(ClaimTypes.Name, cliente.Nombre),
                    new Claim(ClaimTypes.Email, cliente.Email ?? ""),
                    new Claim(ClaimTypes.Role, "Cliente", ClaimValueTypes.String)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    AllowRefresh = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30),
                    IsPersistent = model.recordarme
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                // Redirigir al perfil del cliente o al carrito
                if (!string.IsNullOrWhiteSpace(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Perfil", "Cuenta");
            }
            else
            {
                ViewBag.ReturnUrl = returnUrl;
                ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
                return View(model);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> Perfil()
        {
            var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(emailClaim))
                return RedirectToAction("Login");

            var cliente = await _context.Cliente
                .Where(c => c.Email == emailClaim)
                .FirstOrDefaultAsync();

            if (cliente == null)
                return RedirectToAction("Login");

            return View(cliente);
        }

        [HttpGet]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> MisPedidos()
        {
            var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(emailClaim))
                return RedirectToAction("Login");

            var cliente = await _context.Cliente
                .Where(c => c.Email == emailClaim)
                .FirstOrDefaultAsync();

            if (cliente == null)
                return RedirectToAction("Login");

            var pedidos = await _context.Pedido
                .Where(p => p.IdCliente == cliente.Id)
                //.Include(p => p.DetallePedidos)
                .OrderByDescending(p => p.FechaPedido)
                .ToListAsync();

            return View(pedidos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
/*
        [HttpGet]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> Detalle(int id)
        {
            var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(emailClaim))
                return RedirectToAction("Login");

            var cliente = await _context.Cliente
                .Where(c => c.Email == emailClaim)
                .FirstOrDefaultAsync();

            if (cliente == null)
                return RedirectToAction("Login");

            var pedido = await _context.Pedido
                .Where(p => p.Id == id && p.IdCliente == cliente.Id)
                .Include(p => p.DetallePedidos)
                .ThenInclude(d => d.IdProductoNavigation)
                .FirstOrDefaultAsync();

            if (pedido == null)
                return NotFound();

            return View(pedido);
        }*/
    }
}