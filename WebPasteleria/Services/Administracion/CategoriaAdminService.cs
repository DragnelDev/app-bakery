using Microsoft.EntityFrameworkCore;
using WebPasteleria.Common;
using WebPasteleria.Domain;
using WebPasteleria.Models;
using WebPasteleria.ViewModels;

namespace WebPasteleria.Services.Administracion;

public sealed class CategoriaAdminService : ICategoriaAdminService, ICategoriaLookup
{
    private readonly Models.BakeryContext _db;

    public CategoriaAdminService(Models.BakeryContext db)
    {
        _db = db;
    }

    public async Task<List<CategoriaProducto>> ListarAsync(CancellationToken cancellationToken = default) =>
        await _db.CategoriaProducto
            .AsNoTracking()
            .Where(c => c.Estado != EstadoRegistro.Eliminado)
            .OrderBy(c => c.Nombre)
            .ToListAsync(cancellationToken);

    public Task<List<CategoriaProducto>> ListarActivasAsync(CancellationToken cancellationToken = default) =>
        _db.CategoriaProducto
            .AsNoTracking()
            .Where(c => c.Estado == EstadoRegistro.Activo)
            .OrderBy(c => c.Nombre)
            .ToListAsync(cancellationToken);

    public async Task<CategoriaProducto?> ObtenerAsync(int id, CancellationToken cancellationToken = default) =>
        await _db.CategoriaProducto
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && c.Estado != EstadoRegistro.Eliminado, cancellationToken);

    public async Task<CategoriaFormViewModel?> ObtenerFormularioAsync(int id, CancellationToken cancellationToken = default)
    {
        var categoria = await ObtenerAsync(id, cancellationToken);
        return categoria is null
            ? null
            : new CategoriaFormViewModel { Id = categoria.Id, Nombre = categoria.Nombre, Estado = (byte)categoria.Estado };
    }

    public async Task<OperationResult> CrearAsync(
        CategoriaFormViewModel formulario, string usuario, CancellationToken cancellationToken = default)
    {
        var nombre = formulario.Nombre.Trim();
        if (await ExisteNombreAsync(nombre, null, cancellationToken))
        {
            return OperationResult.Fail("Ya existe una categoría con ese nombre.", nameof(formulario.Nombre));
        }

        _db.CategoriaProducto.Add(new CategoriaProducto { Nombre = nombre, Estado = EstadoRegistro.Activo });
        await _db.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok();
    }

    public async Task<OperationResult> ActualizarAsync(
        int id, CategoriaFormViewModel formulario, CancellationToken cancellationToken = default)
    {
        var categoria = await _db.CategoriaProducto
            .FirstOrDefaultAsync(c => c.Id == id && c.Estado != EstadoRegistro.Eliminado, cancellationToken);
        if (categoria is null)
        {
            return OperationResult.Fail("Categoría no encontrada.");
        }

        var nombre = formulario.Nombre.Trim();
        if (await ExisteNombreAsync(nombre, id, cancellationToken))
        {
            return OperationResult.Fail("Ya existe una categoría con ese nombre.", nameof(formulario.Nombre));
        }

        categoria.Nombre = nombre;
        categoria.Estado = formulario.Estado;
        await _db.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok();
    }

    public async Task<OperationResult> EliminarAsync(int id, CancellationToken cancellationToken = default)
    {
        var categoria = await _db.CategoriaProducto
            .FirstOrDefaultAsync(c => c.Id == id && c.Estado != EstadoRegistro.Eliminado, cancellationToken);
        if (categoria is null)
        {
            return OperationResult.Fail("Categoría no encontrada.");
        }

        var tieneProductos = await _db.Producto
            .AnyAsync(p => p.IdCategoria == id && p.Estado != EstadoRegistro.Eliminado, cancellationToken);
        if (tieneProductos)
        {
            return OperationResult.Fail("No se puede eliminar: la categoría tiene productos asociados.");
        }

        categoria.Estado = EstadoRegistro.Eliminado;
        await _db.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok();
    }

    private Task<bool> ExisteNombreAsync(string nombre, int? idExcluido, CancellationToken cancellationToken) =>
        _db.CategoriaProducto.AnyAsync(
            c => c.Nombre == nombre && c.Estado != EstadoRegistro.Eliminado && c.Id != idExcluido,
            cancellationToken);
}
