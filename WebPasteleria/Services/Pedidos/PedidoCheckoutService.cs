using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebPasteleria.Common;
using WebPasteleria.Configuration;
using WebPasteleria.Domain;
using WebPasteleria.Models;

namespace WebPasteleria.Services.Pedidos;

/// <summary>
/// Convierte una solicitud de checkout en un pedido persistido.
/// Reglas clave: precios y totales siempre se calculan en el servidor, y el stock se descuenta
/// con una sentencia atómica dentro de la misma transacción que crea el pedido.
/// </summary>
public sealed class PedidoCheckoutService : IPedidoCheckoutService
{
    private const int MaxIntentosCodigo = 10;

    private readonly BakeryContext _db;
    private readonly IShippingCostCalculator _envio;
    private readonly ITrackingCodeGenerator _codigos;
    private readonly PedidoOptions _options;
    private readonly TimeProvider _clock;
    private readonly ILogger<PedidoCheckoutService> _logger;

    public PedidoCheckoutService(
        BakeryContext db,
        IShippingCostCalculator envio,
        ITrackingCodeGenerator codigos,
        IOptions<PedidoOptions> options,
        TimeProvider clock,
        ILogger<PedidoCheckoutService> logger)
    {
        _db = db;
        _envio = envio;
        _codigos = codigos;
        _options = options.Value;
        _clock = clock;
        _logger = logger;
    }

    public async Task<OperationResult<PedidoCreado>> CrearAsync(
        CrearPedidoRequest solicitud, string usuarioRegistro, CancellationToken cancellationToken = default)
    {
        var errorEntrega = ValidarEntrega(solicitud);
        if (errorEntrega is not null)
        {
            return OperationResult<PedidoCreado>.Fail(errorEntrega);
        }

        var cantidades = AgruparCantidades(solicitud.Productos);
        if (cantidades.Count == 0)
        {
            return OperationResult<PedidoCreado>.Fail("El pedido debe contener al menos un producto.");
        }

        var productos = await CargarProductosActivosAsync(cantidades.Keys.ToList(), cancellationToken);
        var errorDisponibilidad = ValidarDisponibilidad(cantidades, productos);
        if (errorDisponibilidad is not null)
        {
            return OperationResult<PedidoCreado>.Fail(errorDisponibilidad);
        }

        if (!await _db.MetodoPago.AnyAsync(m => m.Id == solicitud.MetodoPago, cancellationToken))
        {
            return OperationResult<PedidoCreado>.Fail("Método de pago inválido.");
        }

        // Si algo falla antes del Commit, al salir del método la transacción se revierte sola.
        await using var transaccion = await _db.Database.BeginTransactionAsync(cancellationToken);

        var idCliente = await ResolverClienteAsync(solicitud, cancellationToken);

        var errorStock = await ReservarStockAsync(cantidades, productos, cancellationToken);
        if (errorStock is not null)
        {
            return OperationResult<PedidoCreado>.Fail(errorStock);
        }

        var codigo = await GenerarCodigoUnicoAsync(cancellationToken);
        var pedido = ConstruirPedido(solicitud, idCliente, cantidades, productos, codigo, usuarioRegistro);

        _db.Pedido.Add(pedido);
        await _db.SaveChangesAsync(cancellationToken);
        await transaccion.CommitAsync(cancellationToken);

        _logger.LogInformation("Pedido {Codigo} creado por un total de {Total}", codigo, pedido.Total);
        return OperationResult<PedidoCreado>.Ok(new PedidoCreado(pedido.Id, codigo, pedido.Total ?? 0m));
    }

    private static string? ValidarEntrega(CrearPedidoRequest solicitud)
    {
        if (!TipoEntrega.EsValido(solicitud.TipoEntrega))
        {
            return "El tipo de entrega no es válido.";
        }

        if (TipoEntrega.EsDomicilio(solicitud.TipoEntrega) && string.IsNullOrWhiteSpace(solicitud.Direccion))
        {
            return "La dirección es obligatoria para entregas a domicilio.";
        }

        return null;
    }

    /// <summary>Une las líneas repetidas de un mismo producto para validar stock y descontarlo una sola vez.</summary>
    private static Dictionary<int, int> AgruparCantidades(IEnumerable<ItemPedidoRequest>? items) =>
        (items ?? Enumerable.Empty<ItemPedidoRequest>())
            .Where(i => i.Cantidad > 0)
            .GroupBy(i => i.IdProducto)
            .ToDictionary(g => g.Key, g => g.Sum(i => i.Cantidad));

    private async Task<Dictionary<int, Producto>> CargarProductosActivosAsync(
        List<int> ids, CancellationToken cancellationToken) =>
        await _db.Producto
            .AsNoTracking()
            .Where(p => ids.Contains(p.Id) && p.Estado == EstadoRegistro.Activo)
            .ToDictionaryAsync(p => p.Id, cancellationToken);

    private static string? ValidarDisponibilidad(
        IReadOnlyDictionary<int, int> cantidades, IReadOnlyDictionary<int, Producto> productos)
    {
        foreach (var (idProducto, cantidad) in cantidades)
        {
            if (!productos.TryGetValue(idProducto, out var producto))
            {
                return $"El producto con ID {idProducto} no existe o no está disponible.";
            }

            var disponible = producto.Stock ?? 0;
            if (disponible < cantidad)
            {
                return $"Stock insuficiente para {producto.Nombre}. Disponible: {disponible}.";
            }
        }

        return null;
    }

    /// <summary>
    /// Descuenta el stock con un UPDATE condicional (stock >= cantidad). Así dos compras simultáneas
    /// no pueden dejar el stock en negativo aunque pasen ambas la validación previa.
    /// </summary>
    private async Task<string?> ReservarStockAsync(
        IReadOnlyDictionary<int, int> cantidades,
        IReadOnlyDictionary<int, Producto> productos,
        CancellationToken cancellationToken)
    {
        foreach (var (idProducto, cantidad) in cantidades)
        {
            var filasAfectadas = await _db.Producto
                .Where(p => p.Id == idProducto && p.Stock >= cantidad)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Stock, p => p.Stock - cantidad), cancellationToken);

            if (filasAfectadas == 0)
            {
                return $"Stock insuficiente para {productos[idProducto].Nombre}.";
            }
        }

        return null;
    }

    /// <summary>
    /// Vincula el pedido con un cliente existente (por cédula) o registra uno nuevo como invitado.
    /// Los datos de un cliente existente NO se sobrescriben: la solicitud es anónima y cualquiera
    /// podría alterar datos ajenos conociendo una cédula.
    /// </summary>
    private async Task<int?> ResolverClienteAsync(CrearPedidoRequest solicitud, CancellationToken cancellationToken)
    {
        var cedula = solicitud.CedulaIdentidad.LimpiarYTruncar(12);
        if (cedula is null)
        {
            return null;
        }

        var existente = await _db.Cliente.FirstOrDefaultAsync(c => c.CedulaIdentidad == cedula, cancellationToken);
        if (existente is not null)
        {
            return existente.Id;
        }

        var (nombre, apellido) = DividirNombre(solicitud.NombreCliente);
        var nuevo = new Cliente
        {
            CedulaIdentidad = cedula,
            Nombre = nombre,
            Apellido = apellido,
            Email = solicitud.EmailCliente.LimpiarYTruncar(100),
            Celular = solicitud.CelularCliente.LimpiarYTruncar(20),
            Direccion = solicitud.Direccion.LimpiarYTruncar(200),
            TipoCliente = "Regular",
            EsInvitado = true,
            UsuarioRegistro = "Sistema",
            FechaRegistro = _clock.GetLocalNow().DateTime,
            Estado = EstadoRegistro.Activo
        };

        _db.Cliente.Add(nuevo);
        await _db.SaveChangesAsync(cancellationToken);
        return nuevo.Id;
    }

    private static (string Nombre, string Apellido) DividirNombre(string nombreCompleto)
    {
        var partes = nombreCompleto.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var nombre = partes.Length > 0 ? partes[0] : string.Empty;
        var apellido = partes.Length > 1 ? partes[1] : string.Empty;

        return (nombre.LimpiarYTruncar(50) ?? string.Empty, apellido.LimpiarYTruncar(50) ?? string.Empty);
    }

    private async Task<string> GenerarCodigoUnicoAsync(CancellationToken cancellationToken)
    {
        for (var intento = 0; intento < MaxIntentosCodigo; intento++)
        {
            var codigo = _codigos.Generar();
            if (!await _db.Pedido.AnyAsync(p => p.CodigoSeguimiento == codigo, cancellationToken))
            {
                return codigo;
            }
        }

        throw new InvalidOperationException("No fue posible generar un código de seguimiento único.");
    }

    private Pedido ConstruirPedido(
        CrearPedidoRequest solicitud,
        int? idCliente,
        IReadOnlyDictionary<int, int> cantidades,
        IReadOnlyDictionary<int, Producto> productos,
        string codigoSeguimiento,
        string usuarioRegistro)
    {
        var ahora = _clock.GetLocalNow().DateTime;
        var tipoEntrega = solicitud.TipoEntrega.Trim().ToUpperInvariant();
        var diasEntrega = TipoEntrega.EsDomicilio(tipoEntrega)
            ? _options.DiasEntregaDomicilio
            : _options.DiasEntregaRecoger;

        var pedido = new Pedido
        {
            CodigoSeguimiento = codigoSeguimiento,
            FechaPedido = ahora,
            FechaEntrega = ahora.AddDays(diasEntrega),
            IdCliente = idCliente,
            NombreCliente = solicitud.NombreCliente.LimpiarYTruncar(100),
            EmailCliente = solicitud.EmailCliente.LimpiarYTruncar(100),
            CelularCliente = solicitud.CelularCliente.LimpiarYTruncar(20),
            Direccion = TipoEntrega.EsDomicilio(tipoEntrega) ? solicitud.Direccion.LimpiarYTruncar(200) : null,
            IdUsuario = _options.UsuarioSistemaId,
            TipoEntrega = tipoEntrega,
            MetodoPago = solicitud.MetodoPago,
            Observaciones = solicitud.Observaciones.LimpiarYTruncar(500),
            EstadoPedido = PedidoEstado.Pendiente,
            UsuarioRegistro = usuarioRegistro,
            FechaRegistro = ahora,
            Estado = EstadoRegistro.Activo
        };

        var subtotal = 0m;
        foreach (var (idProducto, cantidad) in cantidades)
        {
            var precio = productos[idProducto].Precio;
            subtotal += precio * cantidad;

            pedido.DetallePedido.Add(new DetallePedido
            {
                IdProducto = idProducto,
                Cantidad = cantidad,
                PrecioUnitario = precio,
                Descuento = 0m,
                UsuarioRegistro = usuarioRegistro,
                FechaRegistro = ahora,
                Estado = EstadoRegistro.Activo
            });
        }

        pedido.Total = subtotal + _envio.Calcular(tipoEntrega, solicitud.ZonaEntrega);

        pedido.SeguimientoPedido.Add(new SeguimientoPedido
        {
            EstadoPedido = PedidoEstado.Pendiente,
            Fecha = ahora,
            Descripcion = "Pedido recibido correctamente. Te notificaremos cuando esté confirmado.",
            UsuarioRegistro = "Sistema"
        });

        return pedido;
    }
}
