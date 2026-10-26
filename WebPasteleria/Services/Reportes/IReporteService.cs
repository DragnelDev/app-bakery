using WebPasteleria.ViewModels;

namespace WebPasteleria.Services.Reportes;

public interface IReporteService
{
    Task<ReporteVentasViewModel> VentasAsync(RangoFechasViewModel rango, CancellationToken cancellationToken = default);

    Task<ReporteProductosViewModel> ProductosMasVendidosAsync(
        RangoFechasViewModel rango, int top, CancellationToken cancellationToken = default);

    Task<ReporteClientesViewModel> ClientesFrecuentesAsync(
        RangoFechasViewModel rango, int top, CancellationToken cancellationToken = default);
}

public interface IDashboardService
{
    Task<DashboardViewModel> ObtenerResumenAsync(CancellationToken cancellationToken = default);
}
