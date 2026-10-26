using WebPasteleria.Common;

namespace WebPasteleria.Services.Pedidos;

public sealed record PedidoCreado(int IdPedido, string CodigoSeguimiento, decimal Total);

public interface IPedidoCheckoutService
{
    Task<OperationResult<PedidoCreado>> CrearAsync(
        CrearPedidoRequest solicitud, string usuarioRegistro, CancellationToken cancellationToken = default);
}
