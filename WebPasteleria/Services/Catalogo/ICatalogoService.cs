using WebPasteleria.Models;

namespace WebPasteleria.Services.Catalogo;

public sealed record CatalogoFiltro(string? Categoria, string? Buscar, string? Ordenar);

public sealed record ProductoDetalle(Producto Producto, List<Producto> Relacionados);

public sealed record ProductoBusqueda(int Id, string Nombre, decimal Precio, int? Stock, string ImagenUrl);

public sealed record StockDisponibilidad(bool Disponible, int? StockDisponible, string Mensaje);

/// <summary>Consultas del catálogo público (solo productos activos).</summary>
public interface ICatalogoService
{
    Task<List<Producto>> ListarAsync(CatalogoFiltro filtro, CancellationToken cancellationToken = default);

    Task<List<CategoriaProducto>> ListarCategoriasAsync(CancellationToken cancellationToken = default);

    Task<ProductoDetalle?> ObtenerDetalleAsync(int id, CancellationToken cancellationToken = default);

    Task<List<MetodoPago>> ListarMetodosPagoAsync(CancellationToken cancellationToken = default);

    Task<List<Producto>> ObtenerDestacadosAsync(int cantidad, CancellationToken cancellationToken = default);

    Task<List<ProductoBusqueda>> BuscarAsync(string termino, CancellationToken cancellationToken = default);

    Task<StockDisponibilidad> VerificarStockAsync(int id, int cantidad, CancellationToken cancellationToken = default);
}
