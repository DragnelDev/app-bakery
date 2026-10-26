using Microsoft.EntityFrameworkCore;
using WebPasteleria.Domain;
using WebPasteleria.ViewModels;

namespace WebPasteleria.Services.Reportes;

public sealed class DashboardService : IDashboardService
{
    private const int OrdenesRecientes = 10;

    private readonly Models.BakeryContext _db;
    private readonly TimeProvider _clock;

    public DashboardService(Models.BakeryContext db, TimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<DashboardViewModel> ObtenerResumenAsync(CancellationToken cancellationToken = default)
    {
        var hoy = _clock.GetLocalNow().DateTime.Date;
        var manana = hoy.AddDays(1);
        var pasadoManana = hoy.AddDays(2);

        var vigentes = _db.Pedido
            .AsNoTracking()
            .Where(p => p.Estado != EstadoRegistro.Eliminado);

        var ventasHoy = await vigentes
            .Where(p => p.EstadoPedido != PedidoEstado.Cancelado && p.FechaPedido >= hoy && p.FechaPedido < manana)
            .SumAsync(p => p.Total ?? 0m, cancellationToken);

        var pendientes = await vigentes.CountAsync(p => p.EstadoPedido == PedidoEstado.Pendiente, cancellationToken);

        var entregasManana = await vigentes.CountAsync(
            p => p.FechaEntrega >= manana
                 && p.FechaEntrega < pasadoManana
                 && p.EstadoPedido != PedidoEstado.Entregado
                 && p.EstadoPedido != PedidoEstado.Cancelado,
            cancellationToken);

        var recientes = await vigentes
            .OrderByDescending(p => p.Id)
            .Take(OrdenesRecientes)
            .Select(p => new
            {
                p.Id,
                p.NombreCliente,
                p.Total,
                p.EstadoPedido,
                Primero = p.DetallePedido.OrderBy(d => d.Id).Select(d => d.IdProductoNavigation.Nombre).FirstOrDefault(),
                Cantidad = p.DetallePedido.Count()
            })
            .ToListAsync(cancellationToken);

        return new DashboardViewModel
        {
            VentasHoy = ventasHoy,
            PedidosPendientes = pendientes,
            EntregasManana = entregasManana,
            OrdenesRecientes = recientes.Select(o =>
            {
                var estado = o.EstadoPedido ?? PedidoEstado.Pendiente;
                return new OrdenResumenViewModel(
                    o.Id,
                    o.NombreCliente ?? "Desconocido",
                    DescribirProductos(o.Primero, o.Cantidad),
                    o.Total ?? 0m,
                    estado,
                    PedidoEstado.Describir(estado).Nombre);
            }).ToList()
        };
    }

    private static string DescribirProductos(string? primero, int cantidad) => cantidad switch
    {
        0 => "—",
        1 => primero ?? "—",
        _ => $"{primero} (+{cantidad - 1} más)"
    };
}
