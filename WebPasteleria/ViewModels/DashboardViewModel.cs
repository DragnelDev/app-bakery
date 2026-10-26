namespace WebPasteleria.ViewModels;

public sealed record OrdenResumenViewModel(
    int Id,
    string NombreCliente,
    string Productos,
    decimal Total,
    byte EstadoCodigo,
    string EstadoNombre);

public sealed class DashboardViewModel
{
    public decimal VentasHoy { get; init; }
    public int PedidosPendientes { get; init; }
    public int EntregasManana { get; init; }
    public IReadOnlyList<OrdenResumenViewModel> OrdenesRecientes { get; init; } = Array.Empty<OrdenResumenViewModel>();
}
