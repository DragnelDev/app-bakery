using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebPasteleria.Models;

namespace WebPasteleria.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PedidosController : Controller
    {
        private readonly FinalPasteleriaContext _context;
        private readonly ILogger<PedidosController> _logger;

        public PedidosController(FinalPasteleriaContext context, ILogger<PedidosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Admin/Pedidos
        public async Task<IActionResult> Index()
        {
            var pedidos = await _context.Pedido
                .Include(p => p.IdClienteNavigation)
                .Include(p => p.MetodoPagoNavigation)
                .Where(p => p.Estado != (byte)255)
                .OrderByDescending(p => p.FechaPedido)
                .ToListAsync();

            return View(pedidos);
        }

        // GET: Admin/Pedidos/Detalle/5
        public async Task<IActionResult> Detalle(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pedido = await _context.Pedido
                .Include(p => p.IdClienteNavigation)
                .Include(p => p.MetodoPagoNavigation)
                .Include(p => p.IdUsuarioNavigation)
                .FirstOrDefaultAsync(p => p.Id == id && p.Estado != (byte)255);

            if (pedido == null)
            {
                return NotFound();
            }

            var detalles = await _context.DetallePedido
                .Include(d => d.IdProductoNavigation)
                .Where(d => d.IdPedido == id && d.Estado != (byte)255)
                .ToListAsync();

            var seguimientos = await _context.SeguimientoPedido
                .Where(s => s.IdPedido == id)
                .OrderByDescending(s => s.Fecha)
                .ToListAsync();

            ViewBag.Detalles = detalles;
            ViewBag.Seguimientos = seguimientos;

            return View(pedido);
        }

        // GET: Admin/Pedidos/ActualizarEstado/5
        public async Task<IActionResult> ActualizarEstado(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pedido = await _context.Pedido
                .Include(p => p.IdClienteNavigation)
                .Include(p => p.MetodoPagoNavigation)
                .FirstOrDefaultAsync(p => p.Id == id && p.Estado != (byte)255);

            if (pedido == null)
            {
                return NotFound();
            }

            return View(pedido);
        }

        // POST: Admin/Pedidos/ActualizarEstado
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarEstado(int id, int estadoPedido, string descripcion)
        {
            var pedido = await _context.Pedido.FindAsync(id);

            if (pedido == null)
            {
                TempData["Error"] = "Pedido no encontrado";
                return RedirectToAction(nameof(Index));
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                pedido.EstadoPedido = (byte?)estadoPedido;
                _context.Update(pedido);

                var seguimiento = new SeguimientoPedido
                {
                    IdPedido = pedido.Id,
                    EstadoPedido = (byte)estadoPedido,
                    Fecha = DateTime.Now,
                    Descripcion = descripcion ?? "",
                    UsuarioRegistro = User?.Identity?.Name ?? "Admin"
                };

                _context.SeguimientoPedido.Add(seguimiento);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = "Estado actualizado correctamente";
                return RedirectToAction(nameof(Detalle), new { id = pedido.Id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al actualizar estado del pedido {Id}", id);
                TempData["Error"] = "Error al actualizar el estado";
                return RedirectToAction(nameof(Detalle), new { id = pedido.Id });
            }
        }

        // POST: Admin/Pedidos/Cancelar/5
        [HttpPost]
        public async Task<IActionResult> Cancelar(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var pedido = await _context.Pedido
                    .Include(p => p.IdClienteNavigation)
                    .FirstOrDefaultAsync(p => p.Id == id && p.Estado != (byte)255);

                if (pedido == null)
                {
                    TempData["Error"] = "Pedido no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                if (pedido.EstadoPedido == (byte)5)
                {
                    TempData["Info"] = "El pedido ya fue entregado y no puede ser cancelado.";
                    return RedirectToAction(nameof(Detalle), new { id = id });
                }

                // Actualizar estado a cancelado (6)
                pedido.EstadoPedido = (byte)6;
                _context.Update(pedido);

                // Reponer stock de los productos del pedido
                var detalles = await _context.DetallePedido
                    .Where(d => d.IdPedido == id && d.Estado != (byte)255)
                    .ToListAsync();

                foreach (var d in detalles)
                {
                    var producto = await _context.Producto.FindAsync(d.IdProducto);
                    if (producto != null)
                    {
                        producto.Stock += d.Cantidad;
                        _context.Update(producto);
                    }
                }

                var seguimiento = new SeguimientoPedido
                {
                    IdPedido = pedido.Id,
                    EstadoPedido = (byte)6,
                    Fecha = DateTime.Now,
                    Descripcion = "Pedido cancelado por el administrador.",
                    UsuarioRegistro = User?.Identity?.Name ?? "Admin"
                };

                _context.SeguimientoPedido.Add(seguimiento);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = "Pedido cancelado correctamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al cancelar pedido {Id}", id);
                TempData["Error"] = "Error al cancelar el pedido";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Admin/Pedidos/Imprimir/5
        public async Task<IActionResult> Imprimir(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pedido = await _context.Pedido
                .Include(p => p.IdClienteNavigation)
                .Include(p => p.MetodoPagoNavigation)
                .FirstOrDefaultAsync(p => p.Id == id && p.Estado != (byte)255);

            if (pedido == null)
            {
                return NotFound();
            }

            var detalles = await _context.DetallePedido
                .Include(d => d.IdProductoNavigation)
                .Where(d => d.IdPedido == id && d.Estado != (byte)255)
                .ToListAsync();

            ViewBag.Detalles = detalles;

            return View(pedido);
        }
    }
}
