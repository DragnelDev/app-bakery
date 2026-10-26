using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WebPasteleria.Extensions;
using WebPasteleria.Services.Catalogo;
using WebPasteleria.Services.Pedidos;

namespace WebPasteleria.Controllers;

/// <summary>Flujo público del cliente: checkout, confirmación y seguimiento de pedidos.</summary>
public class PedidosController : Controller
{
    private const string UsuarioRegistroTienda = "Cliente";

    private readonly IPedidoCheckoutService _checkout;
    private readonly IPedidoConsultaPublica _consulta;
    private readonly ICatalogoService _catalogo;
    private readonly ILogger<PedidosController> _logger;

    public PedidosController(
        IPedidoCheckoutService checkout,
        IPedidoConsultaPublica consulta,
        ICatalogoService catalogo,
        ILogger<PedidosController> logger)
    {
        _checkout = checkout;
        _consulta = consulta;
        _catalogo = catalogo;
        _logger = logger;
    }

    public IActionResult BuscarPedido() => View();

    public async Task<IActionResult> Checkout()
    {
        // Los métodos de pago se leen de la BD: el checkout no puede ofrecer opciones que no existen.
        ViewBag.MetodosPago = await _catalogo.ListarMetodosPagoAsync(HttpContext.RequestAborted);
        return View();
    }

    [EnableRateLimiting(PoliticasLimite.ConsultaPublica)]
    public Task<IActionResult> Confirmacion(string? codigo) => MostrarPedidoAsync(codigo, nameof(Confirmacion));

    [EnableRateLimiting(PoliticasLimite.ConsultaPublica)]
    public Task<IActionResult> Seguimiento(string? codigo) => MostrarPedidoAsync(codigo, nameof(Seguimiento));

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(PoliticasLimite.ConsultaPublica)]
    public async Task<IActionResult> BuscarPorEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Alerta("warning", "Por favor ingresa un correo electrónico.");
        }

        var pedidos = await _consulta.BuscarPorEmailAsync(email, HttpContext.RequestAborted);
        if (pedidos.Count == 0)
        {
            return Alerta("info", $"No se encontraron pedidos asociados al correo: {email}");
        }

        ViewBag.EmailBuscado = email;
        return View("ListaPedidosEmail", pedidos);
    }

    /// <summary>
    /// Endpoint JSON del checkout. Valida el antiforgery token (el front lo envía en el header
    /// RequestVerificationToken) y delega todas las reglas en <see cref="IPedidoCheckoutService"/>.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearPedido([FromBody] CrearPedidoRequest solicitud)
    {
        if (!ModelState.IsValid)
        {
            var mensaje = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault(m => !string.IsNullOrWhiteSpace(m)) ?? "Los datos del pedido no son válidos.";

            return BadRequest(new { success = false, mensaje });
        }

        try
        {
            var resultado = await _checkout.CrearAsync(solicitud, UsuarioRegistroTienda, HttpContext.RequestAborted);
            if (!resultado.Succeeded)
            {
                return BadRequest(new { success = false, mensaje = resultado.Error });
            }

            var pedido = resultado.Value!;
            return Json(new
            {
                success = true,
                idPedido = pedido.IdPedido,
                codigoSeguimiento = pedido.CodigoSeguimiento,
                mensaje = "Pedido creado exitosamente"
            });
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error al crear pedido");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                success = false,
                mensaje = "Error al procesar el pedido. Por favor intenta nuevamente."
            });
        }
    }

    private async Task<IActionResult> MostrarPedidoAsync(string? codigo, string vista)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            return RedirectToAction(nameof(BuscarPedido));
        }

        var detalle = await _consulta.ObtenerPorCodigoAsync(codigo, HttpContext.RequestAborted);
        if (detalle is null)
        {
            return Alerta("warning", $"No se encontró un pedido con el código: {codigo}");
        }

        return View(vista, detalle);
    }

    private IActionResult Alerta(string tipo, string mensaje)
    {
        TempData["AlertType"] = tipo;
        TempData["AlertMessage"] = mensaje;
        return RedirectToAction(nameof(BuscarPedido));
    }
}
