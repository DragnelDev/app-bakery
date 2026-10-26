using Microsoft.EntityFrameworkCore;
using WebPasteleria.Domain;
using WebPasteleria.Models;

namespace WebPasteleria.Services.Catalogo;

public sealed class CatalogoService : ICatalogoService
{
    private const string ImagenPorDefecto = "/images/no-image.svg";
    private const int MaxResultadosBusqueda = 10;
    private const int MaxRelacionados = 4;

    private readonly Models.BakeryContext _db;

    public CatalogoService(Models.BakeryContext db)
    {
        _db = db;
    }

    public async Task<List<Producto>> ListarAsync(CatalogoFiltro filtro, CancellationToken cancellationToken = default)
    {
        var consulta = ProductosActivos();

        if (!string.IsNullOrWhiteSpace(filtro.Categoria))
        {
            var categoria = filtro.Categoria.Trim();
            consulta = consulta.Where(p => p.IdCategoriaNavigation.Nombre == categoria);
        }

        if (!string.IsNullOrWhiteSpace(filtro.Buscar))
        {
            var termino = filtro.Buscar.Trim();
            consulta = consulta.Where(p =>
                p.Nombre.Contains(termino)
                || (p.Codigo != null && p.Codigo.Contains(termino))
                || (p.Descripcion != null && p.Descripcion.Contains(termino)));
        }

        consulta = filtro.Ordenar switch
        {
            "name-desc" => consulta.OrderByDescending(p => p.Nombre),
            "price-asc" => consulta.OrderBy(p => p.Precio),
            "price-desc" => consulta.OrderByDescending(p => p.Precio),
            _ => consulta.OrderBy(p => p.Nombre)
        };

        return await consulta.ToListAsync(cancellationToken);
    }

    public async Task<List<CategoriaProducto>> ListarCategoriasAsync(CancellationToken cancellationToken = default) =>
        await _db.CategoriaProducto
            .AsNoTracking()
            .Where(c => c.Estado == EstadoRegistro.Activo)
            .OrderBy(c => c.Nombre)
            .ToListAsync(cancellationToken);

    public async Task<List<MetodoPago>> ListarMetodosPagoAsync(CancellationToken cancellationToken = default) =>
        await _db.MetodoPago.AsNoTracking().OrderBy(m => m.Id).ToListAsync(cancellationToken);

    public async Task<ProductoDetalle?> ObtenerDetalleAsync(int id, CancellationToken cancellationToken = default)
    {
        var producto = await ProductosActivos().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (producto is null)
        {
            return null;
        }

        var relacionados = await ProductosActivos()
            .Where(p => p.IdCategoria == producto.IdCategoria && p.Id != producto.Id)
            .Take(MaxRelacionados)
            .ToListAsync(cancellationToken);

        return new ProductoDetalle(producto, relacionados);
    }

    public async Task<List<Producto>> ObtenerDestacadosAsync(int cantidad, CancellationToken cancellationToken = default) =>
        await ProductosActivos()
            .Where(p => p.Stock > 0)
            .OrderByDescending(p => p.FechaRegistro)
            .Take(cantidad)
            .ToListAsync(cancellationToken);

    public async Task<List<ProductoBusqueda>> BuscarAsync(string termino, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(termino))
        {
            return new List<ProductoBusqueda>();
        }

        var buscado = termino.Trim();
        var productos = await _db.Producto
            .AsNoTracking()
            .Where(p => p.Estado == EstadoRegistro.Activo
                        && (p.Nombre.Contains(buscado) || (p.Codigo != null && p.Codigo.Contains(buscado))))
            .Take(MaxResultadosBusqueda)
            .ToListAsync(cancellationToken);

        return productos
            .Select(p => new ProductoBusqueda(p.Id, p.Nombre, p.Precio, p.Stock, p.ImagenUrl ?? ImagenPorDefecto))
            .ToList();
    }

    public async Task<StockDisponibilidad> VerificarStockAsync(
        int id, int cantidad, CancellationToken cancellationToken = default)
    {
        var producto = await _db.Producto
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && p.Estado == EstadoRegistro.Activo, cancellationToken);

        if (producto is null)
        {
            return new StockDisponibilidad(false, null, "Producto no encontrado");
        }

        var disponible = (producto.Stock ?? 0) >= cantidad;
        return new StockDisponibilidad(
            disponible,
            producto.Stock,
            disponible ? "Stock disponible" : $"Solo hay {producto.Stock ?? 0} unidades disponibles");
    }

    private IQueryable<Producto> ProductosActivos() =>
        _db.Producto
            .AsNoTracking()
            .Include(p => p.IdCategoriaNavigation)
            .Where(p => p.Estado == EstadoRegistro.Activo);
}
