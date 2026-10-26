namespace WebPasteleria.ViewModels;

public sealed record RangoFechasViewModel(DateOnly Desde, DateOnly Hasta);

public sealed record VentaDiariaFila(DateOnly Fecha, int Pedidos, decimal Total);

public sealed class ReporteVentasViewModel
{
    public required RangoFechasViewModel Rango { get; init; }
    public IReadOnlyList<VentaDiariaFila> Filas { get; init; } = Array.Empty<VentaDiariaFila>();
    public decimal TotalGeneral => Filas.Sum(f => f.Total);
    public int TotalPedidos => Filas.Sum(f => f.Pedidos);
}

public sealed record ProductoVendidoFila(int IdProducto, string Nombre, int Unidades, decimal Ingresos);

public sealed class ReporteProductosViewModel
{
    public required RangoFechasViewModel Rango { get; init; }
    public IReadOnlyList<ProductoVendidoFila> Filas { get; init; } = Array.Empty<ProductoVendidoFila>();
}

public sealed record ClienteFrecuenteFila(string Nombre, string Email, int Pedidos, decimal TotalGastado, DateTime UltimoPedido);

public sealed class ReporteClientesViewModel
{
    public required RangoFechasViewModel Rango { get; init; }
    public IReadOnlyList<ClienteFrecuenteFila> Filas { get; init; } = Array.Empty<ClienteFrecuenteFila>();
}
