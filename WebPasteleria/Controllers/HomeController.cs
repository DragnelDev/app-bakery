using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebPasteleria.Configuration;
using WebPasteleria.Models;
using WebPasteleria.Services.Catalogo;
using WebPasteleria.Services.Notificaciones;
using WebPasteleria.ViewModels;

namespace WebPasteleria.Controllers;

public class HomeController : Controller
{
    private const int ProductosDestacados = 12;

    private readonly ICatalogoService _catalogo;
    private readonly IEmailSender _correo;
    private readonly CorreoOptions _opcionesCorreo;
    private readonly ILogger<HomeController> _logger;

    public HomeController(
        ICatalogoService catalogo,
        IEmailSender correo,
        IOptions<CorreoOptions> opcionesCorreo,
        ILogger<HomeController> logger)
    {
        _catalogo = catalogo;
        _correo = correo;
        _opcionesCorreo = opcionesCorreo.Value;
        _logger = logger;
    }

    public async Task<IActionResult> Index() =>
        View(await _catalogo.ObtenerDestacadosAsync(ProductosDestacados, HttpContext.RequestAborted));

    public IActionResult Privacy() => View();

    public IActionResult AcercaDe() => View();

    public IActionResult Contacto() => View();

    public IActionResult Blog() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnviarContacto(ContactoViewModel contacto)
    {
        if (!ModelState.IsValid)
        {
            return Alerta("warning", "Por favor completa correctamente los campos requeridos.");
        }

        if (!_correo.EstaDisponible || string.IsNullOrWhiteSpace(_opcionesCorreo.DestinoContacto))
        {
            _logger.LogWarning("Se recibió un mensaje de contacto pero el correo no está configurado.");
            return Alerta("danger", "En este momento no podemos recibir mensajes por este medio. Llámanos o escríbenos por WhatsApp.");
        }

        var cuerpo = $"Nombre: {contacto.Nombre}\nCorreo: {contacto.Email}\nTeléfono: {contacto.Telefono}\n\n{contacto.Mensaje}";
        var resultado = await _correo.EnviarAsync(
            _opcionesCorreo.DestinoContacto,
            $"Contacto web: {contacto.Nombre}",
            cuerpo,
            contacto.Email,
            HttpContext.RequestAborted);

        return resultado.Succeeded
            ? Alerta("success", "Gracias por contactarnos. Te responderemos pronto.")
            : Alerta("danger", resultado.Error ?? "Hubo un error al enviar tu mensaje.");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

    private RedirectToActionResult Alerta(string tipo, string mensaje)
    {
        TempData["AlertType"] = tipo;
        TempData["AlertMessage"] = mensaje;
        return RedirectToAction(nameof(Contacto));
    }
}
