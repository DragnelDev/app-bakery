using Microsoft.EntityFrameworkCore;
using WebPasteleria.Common;
using WebPasteleria.Domain;
using WebPasteleria.Models;
using WebPasteleria.ViewModels;

namespace WebPasteleria.Services.Administracion;

public sealed class ProductoAdminService : IProductoAdminService
{
    private readonly Models.BakeryContext _db;
    private readonly TimeProvider _clock;

    public ProductoAdminService(Models.BakeryContext db, TimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<List<Producto>> ListarAsync(CancellationToken cancellationToken = default) =>
        await _db.Producto
            .AsNoTracking()
            .Include(p => p.IdCategoriaNavigation)
            .Where(p => p.Estado != EstadoRegistro.Eliminado)
            .OrderByDescending(p => p.FechaRegistro)
            .ToListAsync(cancellationToken);

    public async Task<Producto?> ObtenerAsync(int id, CancellationToken cancellationToken = default) =>
        await _db.Producto
            .AsNoTracking()
            .Include(p => p.IdCategoriaNavigation)
            .FirstOrDefaultAsync(p => p.Id == id && p.Estado != EstadoRegistro.Eliminado, cancellationToken);

    public async Task<ProductoFormViewModel?> ObtenerFormularioAsync(int id, CancellationToken cancellationToken = default)
    {
        var p = await ObtenerAsync(id, cancellationToken);
        return p is null
            ? null
            : new ProductoFormViewModel
            {
                Id = p.Id,
                Codigo = p.Codigo,
                Nombre = p.Nombre,
                IdCategoria = p.IdCategoria,
                UnidadMedida = p.UnidadMedida,
                Precio = p.Precio,
                Stock = p.Stock,
                Tipo = p.Tipo,
                Descripcion = p.Descripcion,
                ImagenUrl = p.ImagenUrl,
                Estado = p.Estado,
                UsuarioRegistro = p.UsuarioRegistro,
                FechaRegistro = p.FechaRegistro
            };
    }

    public async Task<OperationResult> CrearAsync(
        ProductoFormViewModel formulario, string usuario, CancellationToken cancellationToken = default)
    {
        var categoria = await _db.CategoriaProducto.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == formulario.IdCategoria && c.Estado == EstadoRegistro.Activo, cancellationToken);
        if (categoria is null)
        {
            return OperationResult.Fail("Debes seleccionar una categoría válida.", nameof(formulario.IdCategoria));
        }

        var codigo = formulario.Codigo.LimpiarYTruncar(50);
        if (codigo is not null && await CodigoEnUsoAsync(codigo, null, cancellationToken))
        {
            return OperationResult.Fail("Ya existe un producto con ese código.", nameof(formulario.Codigo));
        }

        var producto = new Producto
        {
            Codigo = codigo ?? await GenerarCodigoAsync(categoria.Nombre, cancellationToken),
            UsuarioRegistro = usuario,
            FechaRegistro = _clock.GetLocalNow().DateTime,
            Estado = EstadoRegistro.Activo
        };
        AplicarFormulario(producto, formulario);

        _db.Producto.Add(producto);
        await _db.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok();
    }

    public async Task<OperationResult> ActualizarAsync(
        int id, ProductoFormViewModel formulario, CancellationToken cancellationToken = default)
    {
        var producto = await _db.Producto
            .FirstOrDefaultAsync(p => p.Id == id && p.Estado != EstadoRegistro.Eliminado, cancellationToken);
        if (producto is null)
        {
            return OperationResult.Fail("Producto no encontrado.");
        }

        var categoriaValida = await _db.CategoriaProducto
            .AnyAsync(c => c.Id == formulario.IdCategoria && c.Estado == EstadoRegistro.Activo, cancellationToken);
        if (!categoriaValida)
        {
            return OperationResult.Fail("Debes seleccionar una categoría válida.", nameof(formulario.IdCategoria));
        }

        var codigo = formulario.Codigo.LimpiarYTruncar(50);
        if (codigo is not null && await CodigoEnUsoAsync(codigo, id, cancellationToken))
        {
            return OperationResult.Fail("Ya existe un producto con ese código.", nameof(formulario.Codigo));
        }

        AplicarFormulario(producto, formulario);
        producto.Codigo = codigo ?? producto.Codigo;
        producto.Estado = formulario.Estado;

        await _db.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok();
    }

    public async Task<OperationResult> EliminarAsync(int id, CancellationToken cancellationToken = default)
    {
        var producto = await _db.Producto
            .FirstOrDefaultAsync(p => p.Id == id && p.Estado != EstadoRegistro.Eliminado, cancellationToken);
        if (producto is null)
        {
            return OperationResult.Fail("Producto no encontrado.");
        }

        producto.Estado = EstadoRegistro.Eliminado;
        await _db.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok();
    }

    public async Task<OperationResult<byte>> AlternarEstadoAsync(int id, CancellationToken cancellationToken = default)
    {
        var producto = await _db.Producto
            .FirstOrDefaultAsync(p => p.Id == id && p.Estado != EstadoRegistro.Eliminado, cancellationToken);
        if (producto is null)
        {
            return OperationResult<byte>.Fail("Producto no encontrado.");
        }

        producto.Estado = producto.Estado == EstadoRegistro.Activo ? EstadoRegistro.Inactivo : EstadoRegistro.Activo;
        await _db.SaveChangesAsync(cancellationToken);
        return OperationResult<byte>.Ok(producto.Estado);
    }

    /// <summary>Copia solo los campos editables; auditoría y estado nunca vienen del formulario.</summary>
    private static void AplicarFormulario(Producto producto, ProductoFormViewModel f)
    {
        producto.Nombre = f.Nombre.Trim();
        producto.IdCategoria = f.IdCategoria;
        producto.UnidadMedida = f.UnidadMedida.LimpiarYTruncar(20);
        producto.Precio = f.Precio;
        producto.Stock = f.Stock ?? 0;
        producto.Tipo = f.Tipo.LimpiarYTruncar(50);
        producto.Descripcion = f.Descripcion.LimpiarYTruncar(500);
        producto.ImagenUrl = f.ImagenUrl.LimpiarYTruncar(500);
    }

    // El índice único de la BD incluye los eliminados, por eso aquí no se filtra por estado.
    private Task<bool> CodigoEnUsoAsync(string codigo, int? idExcluido, CancellationToken cancellationToken) =>
        _db.Producto.AnyAsync(p => p.Codigo == codigo && p.Id != idExcluido, cancellationToken);

    private async Task<string> GenerarCodigoAsync(string nombreCategoria, CancellationToken cancellationToken)
    {
        var letras = new string(nombreCategoria.Where(char.IsLetter).Take(3).ToArray()).ToUpperInvariant();
        var prefijo = letras.Length == 3 ? letras : "PRO";
        var siguiente = (await _db.Producto.MaxAsync(p => (int?)p.Id, cancellationToken) ?? 0) + 1;

        string codigo;
        do
        {
            codigo = $"{prefijo}{siguiente:D3}";
            siguiente++;
        }
        while (await _db.Producto.AnyAsync(p => p.Codigo == codigo, cancellationToken));

        return codigo;
    }
}
