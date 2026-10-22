using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using WebPasteleria.Models;

namespace WebPasteleria.Controllers
{
    // Esta etiqueta es común si tu Dashboard es parte de un área de administración
    [Area("Admin")]
    public class DashboardController : Controller
    {
        private readonly FinalPasteleriaContext _context;

        // 📝 Inyección de Dependencias
        // El framework proporciona automáticamente una instancia de FinalPasteleriaContext
        public DashboardController(FinalPasteleriaContext context)
        {
            _context = context;
        }

        // 🎬 Acción principal: Muestra el tablero
        public async Task<IActionResult> Index()
        {
            // --- 1. Obtener Datos del Dashboard ---
            ViewBag.VentasHoy = 450.00m;
            ViewBag.PedidosPendientes = await _context.Pedido
                                                .Where(p => p.EstadoPedido == 1)
                                                .CountAsync();
            ViewBag.EntregasManana = 5;

            // --- 2. Obtener órdenes recientes ---
            var ordenesRecientes = await _context.Pedido
                .OrderByDescending(p => p.Id)
                .Take(10)
                .Select(p => new OrdenViewModel
                {
                    Id = p.Id,
                    NombreCliente = p.NombreCliente ?? "Desconocido",
                    Producto = "Productos varios",
                    Total = p.Total ?? 0,
                    Estado = p.EstadoPedido == 1 ? "Pendiente" : 
                             p.EstadoPedido == 2 ? "Preparacion" : 
                             p.EstadoPedido == 3 ? "Entregado" : "Otro"
                })
                .ToListAsync();

            // --- 3. Devolver la Vista ---
            return View(ordenesRecientes);
        }

        // 🎬 Otra acción de ejemplo: Detalle o Reporte
        public IActionResult ReporteSemanal()
        {
            // Lógica para generar un reporte
            return View();
        }
    }
}