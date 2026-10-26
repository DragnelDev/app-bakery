using Microsoft.EntityFrameworkCore;
using WebPasteleria.Common;
using WebPasteleria.Domain;
using WebPasteleria.Models;
using WebPasteleria.ViewModels;

namespace WebPasteleria.Services.Administracion;

public sealed class EmpleadoAdminService : IEmpleadoAdminService, IEmpleadoLookup
{
    private const int EdadMinima = 18;

    private readonly Models.BakeryContext _db;
    private readonly TimeProvider _clock;

    public EmpleadoAdminService(Models.BakeryContext db, TimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<List<Empleado>> ListarAsync(CancellationToken cancellationToken = default) =>
        await Visibles().OrderByDescending(e => e.FechaRegistro).ToListAsync(cancellationToken);

    public async Task<List<Empleado>> BuscarAsync(string termino, CancellationToken cancellationToken = default)
    {
        var buscado = termino.Trim();
        return await Visibles()
            .Where(e => e.Nombre.Contains(buscado)
                        || e.ApellidoPaterno.Contains(buscado)
                        || (e.ApellidoMaterno != null && e.ApellidoMaterno.Contains(buscado))
                        || e.CedulaIdentidad.Contains(buscado)
                        || e.Email.Contains(buscado))
            .OrderByDescending(e => e.FechaRegistro)
            .ToListAsync(cancellationToken);
    }

    public Task<List<Empleado>> ListarActivosAsync(CancellationToken cancellationToken = default) =>
        _db.Empleado
            .AsNoTracking()
            .Where(e => e.Estado == EstadoRegistro.Activo)
            .OrderBy(e => e.ApellidoPaterno)
            .ThenBy(e => e.Nombre)
            .ToListAsync(cancellationToken);

    public async Task<Empleado?> ObtenerAsync(int id, CancellationToken cancellationToken = default) =>
        await Visibles().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<EmpleadoFormViewModel?> ObtenerFormularioAsync(int id, CancellationToken cancellationToken = default)
    {
        var e = await ObtenerAsync(id, cancellationToken);
        return e is null
            ? null
            : new EmpleadoFormViewModel
            {
                Id = e.Id,
                CedulaIdentidad = e.CedulaIdentidad,
                Nombre = e.Nombre,
                ApellidoPaterno = e.ApellidoPaterno,
                ApellidoMaterno = e.ApellidoMaterno,
                FechaNacimiento = e.FechaNacimiento,
                Direccion = e.Direccion,
                Celular = e.Celular,
                Email = e.Email,
                Cargo = e.Cargo,
                Estado = e.Estado,
                UsuarioRegistro = e.UsuarioRegistro,
                FechaRegistro = e.FechaRegistro
            };
    }

    public async Task<OperationResult> CrearAsync(
        EmpleadoFormViewModel formulario, string usuario, CancellationToken cancellationToken = default)
    {
        var error = await ValidarAsync(formulario, null, cancellationToken);
        if (error is not null)
        {
            return error;
        }

        var empleado = new Empleado
        {
            UsuarioRegistro = usuario,
            FechaRegistro = _clock.GetLocalNow().DateTime,
            Estado = EstadoRegistro.Activo
        };
        AplicarFormulario(empleado, formulario);

        _db.Empleado.Add(empleado);
        await _db.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok();
    }

    public async Task<OperationResult> ActualizarAsync(
        int id, EmpleadoFormViewModel formulario, CancellationToken cancellationToken = default)
    {
        var empleado = await _db.Empleado
            .FirstOrDefaultAsync(e => e.Id == id && e.Estado != EstadoRegistro.Eliminado, cancellationToken);
        if (empleado is null)
        {
            return OperationResult.Fail("Empleado no encontrado.");
        }

        var error = await ValidarAsync(formulario, id, cancellationToken);
        if (error is not null)
        {
            return error;
        }

        AplicarFormulario(empleado, formulario);
        empleado.Estado = formulario.Estado;

        await _db.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok();
    }

    public async Task<OperationResult> EliminarAsync(int id, CancellationToken cancellationToken = default)
    {
        var empleado = await _db.Empleado
            .FirstOrDefaultAsync(e => e.Id == id && e.Estado != EstadoRegistro.Eliminado, cancellationToken);
        if (empleado is null)
        {
            return OperationResult.Fail("Empleado no encontrado.");
        }

        var tieneUsuarios = await _db.Usuario
            .AnyAsync(u => u.IdEmpleado == id && u.Estado != EstadoRegistro.Eliminado, cancellationToken);
        if (tieneUsuarios)
        {
            return OperationResult.Fail("No se puede eliminar este empleado porque tiene usuarios asociados.");
        }

        empleado.Estado = EstadoRegistro.Eliminado;
        await _db.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok();
    }

    private IQueryable<Empleado> Visibles() =>
        _db.Empleado.AsNoTracking().Where(e => e.Estado != EstadoRegistro.Eliminado);

    private async Task<OperationResult?> ValidarAsync(
        EmpleadoFormViewModel f, int? idExcluido, CancellationToken cancellationToken)
    {
        var hoy = DateOnly.FromDateTime(_clock.GetLocalNow().DateTime);
        if (f.FechaNacimiento is null || f.FechaNacimiento.Value > hoy.AddYears(-EdadMinima))
        {
            return OperationResult.Fail(
                $"El empleado debe ser mayor de {EdadMinima} años.", nameof(f.FechaNacimiento));
        }

        var cedula = f.CedulaIdentidad.Trim();
        // El índice único de la cédula incluye eliminados.
        if (await _db.Empleado.AnyAsync(e => e.CedulaIdentidad == cedula && e.Id != idExcluido, cancellationToken))
        {
            return OperationResult.Fail("Esta cédula de identidad ya está registrada.", nameof(f.CedulaIdentidad));
        }

        var email = f.Email.Trim();
        if (await _db.Empleado.AnyAsync(
                e => e.Email == email && e.Estado != EstadoRegistro.Eliminado && e.Id != idExcluido,
                cancellationToken))
        {
            return OperationResult.Fail("Este correo electrónico ya está registrado.", nameof(f.Email));
        }

        return null;
    }

    private static void AplicarFormulario(Empleado empleado, EmpleadoFormViewModel f)
    {
        empleado.CedulaIdentidad = f.CedulaIdentidad.Trim();
        empleado.Nombre = f.Nombre.Trim();
        empleado.ApellidoPaterno = f.ApellidoPaterno.Trim();
        empleado.ApellidoMaterno = f.ApellidoMaterno.LimpiarYTruncar(50);
        empleado.FechaNacimiento = f.FechaNacimiento!.Value;
        empleado.Direccion = f.Direccion.Trim();
        empleado.Celular = f.Celular.Trim();
        empleado.Email = f.Email.Trim();
        empleado.Cargo = f.Cargo.Trim();
    }
}
