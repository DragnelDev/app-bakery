using Microsoft.EntityFrameworkCore;
using WebPasteleria.Common;
using WebPasteleria.Domain;
using WebPasteleria.Models;
using WebPasteleria.Services.Notificaciones;

namespace WebPasteleria.Services.Pedidos;

/// <summary>
/// Cambia el estado de un pedido respetando <see cref="PedidoWorkflow"/> y registra el seguimiento.
/// Cancelar un pedido devuelve el stock reservado, siempre dentro de la misma transacción.
/// </summary>
public sealed class PedidoEstadoService : IPedidoEstadoService
{
    private readonly BakeryContext _db;
    private readonly TimeProvider _clock;
    private readonly IEmailSender _correo;
    private readonly ILogger<PedidoEstadoService> _logger;

    public PedidoEstadoService(
        BakeryContext db,
        TimeProvider clock,
        IEmailSender correo,
        ILogger<PedidoEstadoService> logger)
    {
        _db = db;
        _clock = clock;
        _correo = correo;
        _logger = logger;
    }

    public async Task<OperationResult> CambiarEstadoAsync(
        int idPedido, byte nuevoEstado, string? descripcion, string usuario,
        CancellationToken cancellationToken = default)
    {
        var pedido = await _db.Pedido.FirstOrDefaultAsync(
            p => p.Id == idPedido && p.Estado != EstadoRegistro.Eliminado, cancellationToken);

        if (pedido is null)
        {
            return OperationResult.Fail("Pedido no encontrado.");
        }

        var estadoActual = pedido.EstadoPedido ?? PedidoEstado.Pendiente;
        if (!PedidoWorkflow.PuedeTransicionar(estadoActual, nuevoEstado))
        {
            return OperationResult.Fail(
                $"No se puede cambiar un pedido de '{PedidoEstado.Describir(estadoActual).Nombre}' " +
                $"a '{PedidoEstado.Describir(nuevoEstado).Nombre}'.");
        }

        await using var transaccion = await _db.Database.BeginTransactionAsync(cancellationToken);

        if (nuevoEstado == PedidoEstado.Cancelado)
        {
            await ReponerStockAsync(idPedido, cancellationToken);
        }

        pedido.EstadoPedido = nuevoEstado;
        _db.SeguimientoPedido.Add(new SeguimientoPedido
        {
            IdPedido = pedido.Id,
            EstadoPedido = nuevoEstado,
            Fecha = _clock.GetLocalNow().DateTime,
            Descripcion = descripcion.LimpiarYTruncar(500) ?? PedidoEstado.Describir(nuevoEstado).Descripcion,
            UsuarioRegistro = usuario
        });

        await _db.SaveChangesAsync(cancellationToken);
        await transaccion.CommitAsync(cancellationToken);

        await NotificarClienteAsync(pedido, nuevoEstado, cancellationToken);
        return OperationResult.Ok();
    }

    public Task<OperationResult> CancelarAsync(
        int idPedido, string usuario, CancellationToken cancellationToken = default) =>
        CambiarEstadoAsync(
            idPedido, PedidoEstado.Cancelado, $"Pedido cancelado por {usuario}.", usuario, cancellationToken);

    /// <summary>
    /// Avisa al cliente por correo. Es un efecto secundario: si el correo no está configurado o falla,
    /// el cambio de estado ya quedó guardado y solo se deja constancia en el log.
    /// </summary>
    private async Task NotificarClienteAsync(Pedido pedido, byte estado, CancellationToken cancellationToken)
    {
        if (!_correo.EstaDisponible || string.IsNullOrWhiteSpace(pedido.EmailCliente))
        {
            return;
        }

        var info = PedidoEstado.Describir(estado);
        var cuerpo =
            $"Hola {pedido.NombreCliente},\n\n{info.Descripcion}\n\n" +
            $"Estado actual: {info.Nombre}\nCódigo de seguimiento: {pedido.CodigoSeguimiento}\n\nPastelería Carmen";

        var resultado = await _correo.EnviarAsync(
            pedido.EmailCliente, $"Tu pedido {pedido.CodigoSeguimiento}: {info.Nombre}", cuerpo,
            responderA: null, cancellationToken: cancellationToken);

        if (!resultado.Succeeded)
        {
            _logger.LogWarning("No se pudo notificar el estado del pedido {Codigo}: {Error}", pedido.CodigoSeguimiento, resultado.Error);
        }
    }

    private async Task ReponerStockAsync(int idPedido, CancellationToken cancellationToken)
    {
        var lineas = await _db.DetallePedido
            .Where(d => d.IdPedido == idPedido && d.Estado != EstadoRegistro.Eliminado)
            .Select(d => new { d.IdProducto, d.Cantidad })
            .ToListAsync(cancellationToken);

        foreach (var linea in lineas)
        {
            var cantidad = linea.Cantidad;
            await _db.Producto
                .Where(p => p.Id == linea.IdProducto)
                .ExecuteUpdateAsync(
                    s => s.SetProperty(p => p.Stock, p => (int?)((p.Stock ?? 0) + cantidad)),
                    cancellationToken);
        }
    }
}
