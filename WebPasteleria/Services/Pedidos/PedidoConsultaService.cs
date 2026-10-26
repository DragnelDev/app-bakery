using Microsoft.EntityFrameworkCore;
using WebPasteleria.Domain;
using WebPasteleria.Models;

namespace WebPasteleria.Services.Pedidos;

/// <summary>Implementa las consultas de solo lectura sobre pedidos (cliente y administración).</summary>
public sealed class PedidoConsultaService : IPedidoConsultaPublica, IPedidoConsultaAdmin
{
    private readonly BakeryContext _db;

    public PedidoConsultaService(BakeryContext db)
    {
        _db = db;
    }

    public async Task<PedidoDetalle?> ObtenerPorCodigoAsync(string codigo, CancellationToken cancellationToken = default)
    {
        var normalizado = codigo.Trim().ToUpperInvariant();

        var pedido = await _db.Pedido
            .AsNoTracking()
            .Include(p => p.MetodoPagoNavigation)
            .Include(p => p.IdClienteNavigation)
            .FirstOrDefaultAsync(
                p => p.CodigoSeguimiento == normalizado && p.Estado != EstadoRegistro.Eliminado,
                cancellationToken);

        return pedido is null ? null : await CargarDetalleAsync(pedido, cancellationToken);
    }

    public async Task<List<Pedido>> BuscarPorEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizado = email.Trim();

        return await _db.Pedido
            .AsNoTracking()
            .Include(p => p.MetodoPagoNavigation)
            .Where(p => p.EmailCliente == normalizado && p.Estado != EstadoRegistro.Eliminado)
            .OrderByDescending(p => p.FechaPedido)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Pedido>> ListarAsync(CancellationToken cancellationToken = default) =>
        await _db.Pedido
            .AsNoTracking()
            .Include(p => p.IdClienteNavigation)
            .Include(p => p.MetodoPagoNavigation)
            .Where(p => p.Estado != EstadoRegistro.Eliminado)
            .OrderByDescending(p => p.FechaPedido)
            .ToListAsync(cancellationToken);

    public async Task<PedidoDetalle?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var pedido = await _db.Pedido
            .AsNoTracking()
            .Include(p => p.IdClienteNavigation)
            .Include(p => p.MetodoPagoNavigation)
            .Include(p => p.IdUsuarioNavigation)
            .FirstOrDefaultAsync(p => p.Id == id && p.Estado != EstadoRegistro.Eliminado, cancellationToken);

        return pedido is null ? null : await CargarDetalleAsync(pedido, cancellationToken);
    }

    private async Task<PedidoDetalle> CargarDetalleAsync(Pedido pedido, CancellationToken cancellationToken)
    {
        var detalles = await _db.DetallePedido
            .AsNoTracking()
            .Include(d => d.IdProductoNavigation)
            .Where(d => d.IdPedido == pedido.Id && d.Estado != EstadoRegistro.Eliminado)
            .ToListAsync(cancellationToken);

        var seguimientos = await _db.SeguimientoPedido
            .AsNoTracking()
            .Where(s => s.IdPedido == pedido.Id)
            .OrderByDescending(s => s.Fecha)
            .ToListAsync(cancellationToken);

        return new PedidoDetalle(pedido, detalles, seguimientos);
    }
}
