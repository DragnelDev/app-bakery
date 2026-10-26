using WebPasteleria.Common;
using WebPasteleria.Models;
using WebPasteleria.ViewModels;

namespace WebPasteleria.Services.Administracion;

/// <summary>
/// Contrato común de los mantenimientos (CRUD) del panel. Eliminar es siempre lógico
/// (<c>estado = 255</c>) y los listados nunca devuelven registros eliminados.
/// </summary>
public interface IAdminCrudService<TEntidad, TFormulario>
{
    Task<List<TEntidad>> ListarAsync(CancellationToken cancellationToken = default);

    Task<TEntidad?> ObtenerAsync(int id, CancellationToken cancellationToken = default);

    Task<TFormulario?> ObtenerFormularioAsync(int id, CancellationToken cancellationToken = default);

    Task<OperationResult> CrearAsync(TFormulario formulario, string usuario, CancellationToken cancellationToken = default);

    Task<OperationResult> ActualizarAsync(int id, TFormulario formulario, CancellationToken cancellationToken = default);

    Task<OperationResult> EliminarAsync(int id, CancellationToken cancellationToken = default);
}

public interface IProductoAdminService : IAdminCrudService<Producto, ProductoFormViewModel>
{
    Task<OperationResult<byte>> AlternarEstadoAsync(int id, CancellationToken cancellationToken = default);
}

public interface IClienteAdminService : IAdminCrudService<Cliente, ClienteFormViewModel>
{
}

public interface ICategoriaAdminService : IAdminCrudService<CategoriaProducto, CategoriaFormViewModel>
{
}

public interface IEmpleadoAdminService : IAdminCrudService<Empleado, EmpleadoFormViewModel>
{
    Task<List<Empleado>> BuscarAsync(string termino, CancellationToken cancellationToken = default);
}

public interface IUsuarioAdminService : IAdminCrudService<Usuario, UsuarioFormViewModel>
{
}

/// <summary>Listas de apoyo para los combos de los formularios.</summary>
public interface ICategoriaLookup
{
    Task<List<CategoriaProducto>> ListarActivasAsync(CancellationToken cancellationToken = default);
}

public interface IEmpleadoLookup
{
    Task<List<Empleado>> ListarActivosAsync(CancellationToken cancellationToken = default);
}
