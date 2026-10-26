using Microsoft.EntityFrameworkCore;
using WebPasteleria.Domain;
using WebPasteleria.Models;
using WebPasteleria.ViewModels;

namespace WebPasteleria.Services.Reportes;

/// <summary>
/// Reportes administrativos. Solo cuentan pedidos vigentes (no eliminados ni cancelados).
/// Se traen únicamente las columnas necesarias y se agrupa en memoria: el volumen de una
/// pastelería lo permite y evita traducciones SQL frágiles.
/// </summary>
public sealed class ReporteService : IReporteService
{
    private readonly BakeryContext _db;

    public ReporteService(BakeryContext db)
    {
        _db = db;
    }

    public async Task<ReporteVentasViewModel> VentasAsync(
        RangoFechasViewModel rango, CancellationToken cancellationToken = default)
    {
        var (inicio, fin) = Limites(rango);

        var pedidos = await PedidosVigentes(inicio, fin)
            .Select(p => new { p.FechaPedido, p.Total })
            .ToListAsync(cancellationToken);

        var filas = pedidos
            .GroupBy(p => DateOnly.FromDateTime(p.FechaPedido!.Value))
            .OrderBy(g => g.Key)
            .Select(g => new VentaDiariaFila(g.Key, g.Count(), g.Sum(p => p.Total ?? 0m)))
            .ToList();

        return new ReporteVentasViewModel { Rango = rango, Filas = filas };
    }

    public async Task<ReporteProductosViewModel> ProductosMasVendidosAsync(
        RangoFechasViewModel rango, int top, CancellationToken cancellationToken = default)
    {
        var (inicio, fin) = Limites(rango);

        var lineas = await _db.DetallePedido
            .AsNoTracking()
            .Where(d => d.Estado != EstadoRegistro.Eliminado
                        && d.IdPedidoNavigation.Estado != EstadoRegistro.Eliminado
                        && d.IdPedidoNavigation.EstadoPedido != PedidoEstado.Cancelado
                        && d.IdPedidoNavigation.FechaPedido >= inicio
                        && d.IdPedidoNavigation.FechaPedido < fin)
            .Select(d => new { d.IdProducto, d.IdProductoNavigation.Nombre, d.Cantidad, d.SubTotal })
            .ToListAsync(cancellationToken);

        var filas = lineas
            .GroupBy(l => new { l.IdProducto, l.Nombre })
            .Select(g => new ProductoVendidoFila(
                g.Key.IdProducto, g.Key.Nombre, g.Sum(l => l.Cantidad), g.Sum(l => l.SubTotal ?? 0m)))
            .OrderByDescending(f => f.Unidades)
            .ThenByDescending(f => f.Ingresos)
            .Take(top)
            .ToList();

        return new ReporteProductosViewModel { Rango = rango, Filas = filas };
    }

    public async Task<ReporteClientesViewModel> ClientesFrecuentesAsync(
        RangoFechasViewModel rango, int top, CancellationToken cancellationToken = default)
    {
        var (inicio, fin) = Limites(rango);

        var pedidos = await PedidosVigentes(inicio, fin)
            .Where(p => p.EmailCliente != null)
            .Select(p => new { p.EmailCliente, p.NombreCliente, p.Total, p.FechaPedido })
            .ToListAsync(cancellationToken);

        // Los clientes invitados no tienen IdCliente: el correo es el identificador común.
        var filas = pedidos
            .GroupBy(p => p.EmailCliente!.Trim().ToLowerInvariant())
            .Select(g => new ClienteFrecuenteFila(
                g.OrderByDescending(p => p.FechaPedido).First().NombreCliente ?? g.Key,
                g.Key,
                g.Count(),
                g.Sum(p => p.Total ?? 0m),
                g.Max(p => p.FechaPedido) ?? DateTime.MinValue))
            .OrderByDescending(f => f.Pedidos)
            .ThenByDescending(f => f.TotalGastado)
            .Take(top)
            .ToList();

        return new ReporteClientesViewModel { Rango = rango, Filas = filas };
    }

    private IQueryable<Pedido> PedidosVigentes(DateTime inicio, DateTime fin) =>
        _db.Pedido
            .AsNoTracking()
            .Where(p => p.Estado != EstadoRegistro.Eliminado
                        && p.EstadoPedido != PedidoEstado.Cancelado
                        && p.FechaPedido >= inicio
                        && p.FechaPedido < fin);

    /// <summary>Convierte el rango de fechas (inclusivo) en límites [inicio, fin) de DateTime.</summary>
    private static (DateTime Inicio, DateTime Fin) Limites(RangoFechasViewModel rango) =>
        (rango.Desde.ToDateTime(TimeOnly.MinValue), rango.Hasta.AddDays(1).ToDateTime(TimeOnly.MinValue));
}
