using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebPasteleria.Models;

namespace WebPasteleria.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly FinalPasteleriaContext _context;

        public HomeController(ILogger<HomeController> logger, FinalPasteleriaContext context)
        {
            _logger = logger;
            _context = context;
        }

        // GET: Home/Index - P�gina principal
        public async Task<IActionResult> Index()
        {
            // Obtener productos destacados (activos, con stock, ordenados por fecha de registro)
            var productosDestacados = await _context.Producto
                .Include(p => p.IdCategoriaNavigation)
                .Where(p => p.Estado == 1 && p.Stock > 0)
                .OrderByDescending(p => p.FechaRegistro)
                .Take(12) // Mostrar solo 12 productos en la p�gina de inicio
                .ToListAsync();

            return View(productosDestacados);
        }

        // GET: Home/Producto
        public IActionResult Producto()
        {
            return View();
        }

        // GET: Home/Privacy
        public IActionResult Privacy()
        {
            return View();
        }

        // GET: Home/AcercaDe
        public IActionResult AcercaDe()
        {
            return View();
        }

        // GET: Home/Contacto
        public IActionResult Contacto()
        {
            return View();
        }

        // GET: Home/Blog
        public IActionResult Blog()
        {
            return View();
        }
        
        public IActionResult Cuenta()
        {
            return View();
        }

        // GET: Home/Carrito
        public IActionResult Carrito()
        {
            // Renderizar la vista del carrito ubicada en Views/Carrito/Index.cshtml
            return View("~/Views/Carrito/Index.cshtml");
        }

        // GET: Home/Checkout
        public IActionResult Checkout()
        {
            return View("~/Views/Pedidos/Checkout.cshtml");
        }

        // POST: Home/EnviarContacto
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EnviarContacto(string nombre, string email, string telefono, string mensaje)
        {
            try
            {
                // Aqu� puedes implementar la l�gica para enviar el correo o guardar en BD
                // Por ahora solo mostramos un mensaje de �xito

                if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(mensaje))
                {
                    TempData["AlertType"] = "warning";
                    TempData["AlertMessage"] = "Por favor complete todos los campos requeridos";
                    return RedirectToAction(nameof(Contacto));
                }

                // TODO: Implementar env�o de correo electr�nico
                // await _emailService.EnviarCorreoContacto(nombre, email, telefono, mensaje);

                TempData["AlertType"] = "success";
                TempData["AlertMessage"] = "Gracias por contactarnos. Te responderemos pronto.";

                return RedirectToAction(nameof(Contacto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar formulario de contacto");
                TempData["AlertType"] = "danger";
                TempData["AlertMessage"] = "Hubo un error al enviar tu mensaje. Por favor intenta nuevamente.";
                return RedirectToAction(nameof(Contacto));
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}