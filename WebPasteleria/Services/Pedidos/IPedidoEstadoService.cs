using WebPasteleria.Common;

namespace WebPasteleria.Services.Pedidos;

public interface IPedidoEstadoService
{
    Task<OperationResult> CambiarEstadoAsync(
        int idPedido, byte nuevoEstado, string? descripcion, string usuario,
        CancellationToken cancellationToken = default);

    Task<OperationResult> CancelarAsync(int idPedido, string usuario, CancellationToken cancellationToken = default);
}
