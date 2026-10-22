using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Security.Cryptography;
using WebPasteleria.Models;

namespace WebPasteleria.Controllers
{
    public class PedidosController : Controller
    {
        private readonly FinalPasteleriaContext _context;
        private readonly ILogger<PedidosController> _logger;

        public PedidosController(FinalPasteleriaContext context, ILogger<PedidosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // =============================================
        // ÁREA PÚBLICA - CLIENTE
        // =============================================

        // GET: Pedidos/BuscarPedido
        public IActionResult BuscarPedido()
        {
            return View();
        }

        // GET: Pedidos/Checkout
        public IActionResult Checkout()
        {
            return View();
        }

        // GET: Pedidos/Confirmacion
        public IActionResult Confirmacion()
        {
            return View();
        }

        // GET: Pedidos/Seguimiento?codigo=ABC123
        public async Task<IActionResult> Seguimiento(string? codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
            {
                return RedirectToAction(nameof(BuscarPedido));
            }

            var pedido = await _context.Pedido
                .Include(p => p.MetodoPagoNavigation)
                .Include(p => p.IdClienteNavigation)
                .FirstOrDefaultAsync(p => p.CodigoSeguimiento == codigo.ToUpper() && p.Estado != (byte)255);

            if (pedido == null)
            {
                TempData["AlertType"] = "warning";
                TempData["AlertMessage"] = $"No se encontró un pedido con el código: {codigo}";
                return RedirectToAction(nameof(BuscarPedido));
            }

            // Obtener seguimientos
            var seguimientos = await _context.SeguimientoPedido
                .Where(s => s.IdPedido == pedido.Id)
                .OrderByDescending(s => s.Fecha)
                .ToListAsync();

            // Obtener detalles del pedido
            var detalles = await _context.DetallePedido
                .Include(d => d.IdProductoNavigation)
                .Where(d => d.IdPedido == pedido.Id && d.Estado != (byte)255)
                .ToListAsync();

            ViewBag.Seguimientos = seguimientos;
            ViewBag.Detalles = detalles;

            return View(pedido);
        }

        // POST: Pedidos/BuscarPorEmail
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BuscarPorEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertMessage"] = "Por favor ingresa un correo electrónico";
                return RedirectToAction(nameof(BuscarPedido));
            }

            var emailTrim = email.Trim();
            var pedidos = await _context.Pedido
                .Include(p => p.MetodoPagoNavigation)
                .Where(p => p.EmailCliente.ToLower() == emailTrim.ToLower() && p.Estado != (byte)255)
                .OrderByDescending(p => p.FechaPedido)
                .ToListAsync();

            if (!pedidos.Any())
            {
                TempData["AlertType"] = "info";
                TempData["AlertMessage"] = $"No se encontraron pedidos asociados al correo: {email}";
                return RedirectToAction(nameof(BuscarPedido));
            }

            ViewBag.EmailBuscado = email;
            return View("ListaPedidosEmail", pedidos);
        }

        // DTO para recibir el pedido desde el frontend
        public class CrearPedidoDto
        {
            public string NombreCliente { get; set; }
            public string EmailCliente { get; set; }
            public string CelularCliente { get; set; }
            public string? CedulaIdentidad { get; set; }
            public string TipoEntrega { get; set; }
            public string? Direccion { get; set; }
            public int MetodoPago { get; set; }
            public string? Observaciones { get; set; }
            public List<ProductoPedidoDto> Productos { get; set; }
            public decimal Subtotal { get; set; }
            public decimal Descuento { get; set; }
            public decimal CostoEnvio { get; set; }
            public decimal Total { get; set; }
        }

        public class ProductoPedidoDto
        {
            public int IdProducto { get; set; }
            public int Cantidad { get; set; }
            public decimal PrecioUnitario { get; set; }
        }

        // POST: Pedidos/CrearPedido
        [HttpPost]
        [IgnoreAntiforgeryToken]  // Para endpoints JSON públicos, la validación se maneja diferente
        public async Task<IActionResult> CrearPedido([FromBody] CrearPedidoDto pedidoDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Validaciones básicas
                if (string.IsNullOrWhiteSpace(pedidoDto.NombreCliente) ||
                    string.IsNullOrWhiteSpace(pedidoDto.EmailCliente) ||
                    string.IsNullOrWhiteSpace(pedidoDto.CelularCliente))
                {
                    return Json(new { success = false, mensaje = "Datos del cliente incompletos" });
                }

                if (pedidoDto.Productos == null || !pedidoDto.Productos.Any())
                {
                    return Json(new { success = false, mensaje = "El pedido debe contener al menos un producto" });
                }

                // Verificar stock de productos
                foreach (var item in pedidoDto.Productos)
                {
                    var producto = await _context.Producto.FindAsync(item.IdProducto);
                    if (producto == null)
                    {
                        return Json(new { success = false, mensaje = $"Producto con ID {item.IdProducto} no encontrado" });
                    }

                    if (producto.Stock < item.Cantidad)
                    {
                        return Json(new { success = false, mensaje = $"Stock insuficiente para {producto.Nombre}. Disponible: {producto.Stock}" });
                    }
                }

                // Buscar o crear cliente
                int? idCliente = null;
                if (!string.IsNullOrWhiteSpace(pedidoDto.CedulaIdentidad))
                {
                    var clienteExistente = await _context.Cliente
                        .FirstOrDefaultAsync(c => c.CedulaIdentidad == pedidoDto.CedulaIdentidad);

                    if (clienteExistente != null)
                    {
                        idCliente = clienteExistente.Id;

                        // Actualizar datos del cliente
                        var partes = pedidoDto.NombreCliente?.Trim().Split(' ', 2);
                        clienteExistente.Nombre = partes != null && partes.Length > 0 ? partes[0] : clienteExistente.Nombre;
                        clienteExistente.Apellido = partes != null && partes.Length > 1 ? partes[1] : clienteExistente.Apellido;
                        clienteExistente.Email = pedidoDto.EmailCliente;
                        clienteExistente.Celular = pedidoDto.CelularCliente;
                        if (!string.IsNullOrWhiteSpace(pedidoDto.Direccion))
                        {
                            clienteExistente.Direccion = pedidoDto.Direccion;
                        }
                        _context.Update(clienteExistente);
                    }
                    else
                    {
                        // Crear nuevo cliente
                        var partesNuevo = pedidoDto.NombreCliente?.Trim().Split(' ', 2);
                        var nuevoCliente = new Cliente
                        {
                            CedulaIdentidad = pedidoDto.CedulaIdentidad,
                            Nombre = partesNuevo != null && partesNuevo.Length > 0 ? partesNuevo[0] : "",
                            Apellido = partesNuevo != null && partesNuevo.Length > 1 ? partesNuevo[1] : "",
                            Email = pedidoDto.EmailCliente,
                            Celular = pedidoDto.CelularCliente,
                            Direccion = pedidoDto.Direccion,
                            TipoCliente = "Regular",
                            UsuarioRegistro = "Sistema",
                            FechaRegistro = DateTime.Now,
                            Estado = 1
                        };

                        _context.Cliente.Add(nuevoCliente);
                        await _context.SaveChangesAsync();
                        idCliente = nuevoCliente.Id;
                    }
                }

                // Validar método de pago
                var metodoPago = await _context.MetodoPago.FindAsync(pedidoDto.MetodoPago);
                if (metodoPago == null || (metodoPago.GetType().GetProperty("Estado") != null && (int)metodoPago.GetType().GetProperty("Estado").GetValue(metodoPago) == -1))
                {
                    return Json(new { success = false, mensaje = "Método de pago inválido" });
                }

                // Generar código de seguimiento único
                string codigoSeguimiento = await GenerarCodigoSeguimientoAsync();

                // Obtener usuario actual (si está autenticado) o usar un usuario por defecto
                int idUsuario = 1; // Usuario por defecto del sistema

                // Crear el pedido
                var pedido = new Pedido
                {
                    CodigoSeguimiento = codigoSeguimiento,
                    FechaPedido = DateTime.Now,
                    FechaEntrega = DateTime.Now.AddDays(pedidoDto.TipoEntrega == "RECOGER" ? 1 : 2),
                    IdCliente = idCliente,
                    NombreCliente = pedidoDto.NombreCliente,
                    EmailCliente = pedidoDto.EmailCliente,
                    CelularCliente = pedidoDto.CelularCliente,
                    IdUsuario = idUsuario,
                    TipoEntrega = pedidoDto.TipoEntrega,
                    MetodoPago = pedidoDto.MetodoPago,
                    Observaciones = pedidoDto.Observaciones,
                    Total = pedidoDto.Total,
                    EstadoPedido = (byte?)0,
                    UsuarioRegistro = "Cliente",
                    FechaRegistro = DateTime.Now,
                    Estado = 1
                };

                _context.Pedido.Add(pedido);
                await _context.SaveChangesAsync();

                // Crear detalles del pedido y actualizar stock
                foreach (var item in pedidoDto.Productos)
                {
                    var producto = await _context.Producto.FindAsync(item.IdProducto);
                    if (producto != null)
                    {
                        var detalle = new DetallePedido
                        {
                            IdPedido = pedido.Id,
                            IdProducto = producto.Id,
                            Cantidad = item.Cantidad,
                            PrecioUnitario = item.PrecioUnitario,
                            Estado = 1
                        };

                        _context.DetallePedido.Add(detalle);

                        producto.Stock -= item.Cantidad;
                        _context.Update(producto);
                    }
                }

                // Crear registro de seguimiento inicial
                var seguimiento = new SeguimientoPedido
                {
                    IdPedido = pedido.Id,
                    EstadoPedido = (byte)0,
                    Fecha = DateTime.Now,
                    Descripcion = "Pedido recibido correctamente. Estamos procesando tu solicitud y te notificaremos cuando esté confirmado.",
                    UsuarioRegistro = "Sistema"
                };

                _context.SeguimientoPedido.Add(seguimiento);
                await _context.SaveChangesAsync();

                // Confirmar transacción
                await transaction.CommitAsync();

                _logger.LogInformation($"Pedido {codigoSeguimiento} creado exitosamente para {pedidoDto.EmailCliente}");

                return Json(new
                {
                    success = true,
                    idPedido = pedido.Id,
                    codigoSeguimiento = codigoSeguimiento,
                    mensaje = "Pedido creado exitosamente"
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al crear pedido");

                return Json(new
                {
                    success = false,
                    mensaje = "Error al procesar el pedido. Por favor intenta nuevamente."
                });
            }
        }

        // GET: Pedidos/Confirmacion/5
        public async Task<IActionResult> Confirmacion(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pedido = await _context.Pedido
                .Include(p => p.IdClienteNavigation)
                .Include(p => p.MetodoPagoNavigation)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null)
            {
                return NotFound();
            }

            // Obtener detalles del pedido
            var detalles = await _context.DetallePedido
                .Include(d => d.IdProductoNavigation)
                .Where(d => d.IdPedido == id && d.Estado != (byte)255)
                .ToListAsync();

            ViewBag.Detalles = detalles;

            return View(pedido);
        }

        // Metodo para generar codigo unico para el seguimiento - pedido
        private async Task<string> GenerarCodigoSeguimientoAsync()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            const int length = 8;
            const int maxAttempts = 20;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                var buffer = new char[length];
                using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
                {
                    var bytes = new byte[length];
                    rng.GetBytes(bytes);
                    for (int i = 0; i < length; i++)
                    {
                        buffer[i] = chars[bytes[i] % chars.Length];
                    }
                }

                var codigo = new string(buffer);

                var exists = await _context.Pedido.AnyAsync(p => p.CodigoSeguimiento == codigo);
                if (!exists)
                {
                    return codigo;
                }
            }

            // Fallback: GUID corto en mayúsculas
            return Guid.NewGuid().ToString("N").Substring(0, length).ToUpperInvariant();
        }

        // Método auxiliar para enviar notificaciones (TODO: implementar)
        /*
        private async Task EnviarNotificacionCambioEstado(Pedido pedido, int nuevoEstado, string descripcion)
        {
            var estadoTexto = nuevoEstado switch
            {
                0 => "Pendiente",
                1 => "Confirmado",
                2 => "En Preparación",
                3 => "Listo para Entregar",
                4 => "En Camino",
                5 => "Entregado",
                6 => "Cancelado",
                _ => "Desconocido"
            };

            var emailBody = $@"
                <h2>Actualización de tu Pedido</h2>
                <p>Hola {pedido.NombreCliente},</p>
                <p>Tu pedido <strong>{pedido.CodigoSeguimiento}</strong> ha cambiado de estado:</p>
                <p><strong>Nuevo Estado:</strong> {estadoTexto}</p>
                <p><strong>Descripción:</strong> {descripcion}</p>
                <p>Puedes ver el seguimiento completo en: 
                   <a href='https://tudominio.com/Pedidos/Seguimiento?codigo={pedido.CodigoSeguimiento}'>
                   Ver seguimiento
                   </a>
                </p>
            ";

            // Implementar envío de email aquí
            await Task.CompletedTask;
        }
        */
    }
}