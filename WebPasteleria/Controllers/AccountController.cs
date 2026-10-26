using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WebPasteleria.Extensions;
using WebPasteleria.Services.Security;
using WebPasteleria.ViewModels;

namespace WebPasteleria.Controllers;

/// <summary>Inicio y cierre de sesión del personal del panel de administración.</summary>
[Authorize]
public class AccountController : Controller
{
    public const string ClaimNombreCompleto = "FullName";

    private readonly IAuthService _auth;

    public AccountController(IAuthService auth)
    {
        _auth = auth;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirigirTrasLogin(returnUrl);
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(PoliticasLimite.Login)]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var resultado = await _auth.AutenticarAsync(model.Usuario.Trim(), model.Clave, HttpContext.RequestAborted);
        if (!resultado.Succeeded)
        {
            ModelState.AddModelError(string.Empty, resultado.Error!);
            return View(model);
        }

        var usuario = resultado.Value!;
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.UserName),
            new(ClaimNombreCompleto, usuario.FullName),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Role, usuario.Role)
        };

        var identidad = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identidad),
            new AuthenticationProperties { IsPersistent = model.Recordarme });

        return RedirigirTrasLogin(returnUrl);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(HomeController.Index), "Home");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccesoDenegado() => View();

    // Url.IsLocalUrl evita que un enlace manipulado redirija al usuario a un sitio externo (open redirect).
    private IActionResult RedirigirTrasLogin(string? returnUrl) =>
        !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToAction("Index", "Dashboard", new { area = "Admin" });
}
