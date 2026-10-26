using WebPasteleria.Models;

namespace WebPasteleria.Services.Pedidos;

/// <summary>Un pedido junto con sus líneas y su historial de seguimiento.</summary>
public sealed record PedidoDetalle(
    Pedido Pedido,
    List<DetallePedido> Detalles,
    List<SeguimientoPedido> Seguimientos);

/// <summary>Consultas que puede hacer un cliente anónimo desde la tienda.</summary>
public interface IPedidoConsultaPublica
{
    Task<PedidoDetalle?> ObtenerPorCodigoAsync(string codigo, CancellationToken cancellationToken = default);

    Task<List<Pedido>> BuscarPorEmailAsync(string email, CancellationToken cancellationToken = default);
}

/// <summary>Consultas del panel de administración.</summary>
public interface IPedidoConsultaAdmin
{
    Task<List<Pedido>> ListarAsync(CancellationToken cancellationToken = default);

    Task<PedidoDetalle?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
}
