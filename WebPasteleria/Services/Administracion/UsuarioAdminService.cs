using Microsoft.EntityFrameworkCore;
using WebPasteleria.Common;
using WebPasteleria.Domain;
using WebPasteleria.Models;
using WebPasteleria.Services.Security;
using WebPasteleria.ViewModels;

namespace WebPasteleria.Services.Administracion;

public sealed class UsuarioAdminService : IUsuarioAdminService
{
    private readonly Models.BakeryContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly TimeProvider _clock;

    public UsuarioAdminService(Models.BakeryContext db, IPasswordHasher hasher, TimeProvider clock)
    {
        _db = db;
        _hasher = hasher;
        _clock = clock;
    }

    public async Task<List<Usuario>> ListarAsync(CancellationToken cancellationToken = default) =>
        await _db.Usuario
            .AsNoTracking()
            .Include(u => u.IdEmpleadoNavigation)
            .Where(u => u.Estado != EstadoRegistro.Eliminado)
            .OrderBy(u => u.Usuario1)
            .ToListAsync(cancellationToken);

    public async Task<Usuario?> ObtenerAsync(int id, CancellationToken cancellationToken = default) =>
        await _db.Usuario
            .AsNoTracking()
            .Include(u => u.IdEmpleadoNavigation)
            .FirstOrDefaultAsync(u => u.Id == id && u.Estado != EstadoRegistro.Eliminado, cancellationToken);

    public async Task<UsuarioFormViewModel?> ObtenerFormularioAsync(int id, CancellationToken cancellationToken = default)
    {
        var u = await ObtenerAsync(id, cancellationToken);
        return u is null
            ? null
            : new UsuarioFormViewModel
            {
                Id = u.Id,
                IdEmpleado = u.IdEmpleado,
                NombreUsuario = u.Usuario1,
                Rol = u.Role,
                Estado = u.Estado,
                IntentosFallidos = u.IntentosFallidos ?? 0,
                UltimoLogin = u.UltimoLogin
            };
    }

    public async Task<OperationResult> CrearAsync(
        UsuarioFormViewModel formulario, string usuario, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(formulario.Clave))
        {
            return OperationResult.Fail("La contraseña es obligatoria.", nameof(formulario.Clave));
        }

        var error = await ValidarAsync(formulario, null, cancellationToken);
        if (error is not null)
        {
            return error;
        }

        _db.Usuario.Add(new Usuario
        {
            IdEmpleado = formulario.IdEmpleado,
            Usuario1 = formulario.NombreUsuario.Trim(),
            Clave = _hasher.Hash(formulario.Clave),
            Role = formulario.Rol,
            IntentosFallidos = 0,
            UsuarioRegistro = usuario,
            FechaRegistro = _clock.GetLocalNow().DateTime,
            Estado = EstadoRegistro.Activo
        });

        await _db.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok();
    }

    public async Task<OperationResult> ActualizarAsync(
        int id, UsuarioFormViewModel formulario, CancellationToken cancellationToken = default)
    {
        var cuenta = await _db.Usuario
            .FirstOrDefaultAsync(u => u.Id == id && u.Estado != EstadoRegistro.Eliminado, cancellationToken);
        if (cuenta is null)
        {
            return OperationResult.Fail("Usuario no encontrado.");
        }

        var error = await ValidarAsync(formulario, id, cancellationToken);
        if (error is not null)
        {
            return error;
        }

        var seguiraSiendoAdminActivo = formulario.Rol == Roles.Administrador && formulario.Estado == EstadoRegistro.Activo;
        if (!seguiraSiendoAdminActivo && !await QuedaOtroAdministradorActivoAsync(id, cancellationToken))
        {
            return OperationResult.Fail("Debe existir al menos un administrador activo.");
        }

        cuenta.IdEmpleado = formulario.IdEmpleado;
        cuenta.Usuario1 = formulario.NombreUsuario.Trim();
        cuenta.Role = formulario.Rol;
        cuenta.Estado = formulario.Estado;

        if (!string.IsNullOrWhiteSpace(formulario.Clave))
        {
            cuenta.Clave = _hasher.Hash(formulario.Clave);
            cuenta.IntentosFallidos = 0;
        }

        if (formulario.ReiniciarIntentos)
        {
            cuenta.IntentosFallidos = 0;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok();
    }

    public async Task<OperationResult> EliminarAsync(int id, CancellationToken cancellationToken = default)
    {
        var cuenta = await _db.Usuario
            .FirstOrDefaultAsync(u => u.Id == id && u.Estado != EstadoRegistro.Eliminado, cancellationToken);
        if (cuenta is null)
        {
            return OperationResult.Fail("Usuario no encontrado.");
        }

        if (!await QuedaOtroAdministradorActivoAsync(id, cancellationToken))
        {
            return OperationResult.Fail("Debe existir al menos un administrador activo.");
        }

        cuenta.Estado = EstadoRegistro.Eliminado;
        await _db.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok();
    }

    private async Task<OperationResult?> ValidarAsync(
        UsuarioFormViewModel f, int? idExcluido, CancellationToken cancellationToken)
    {
        if (!Roles.Asignables.Contains(f.Rol))
        {
            return OperationResult.Fail("El rol seleccionado no es válido.", nameof(f.Rol));
        }

        var empleadoActivo = await _db.Empleado
            .AnyAsync(e => e.Id == f.IdEmpleado && e.Estado == EstadoRegistro.Activo, cancellationToken);
        if (!empleadoActivo)
        {
            return OperationResult.Fail("El empleado seleccionado no existe o está inactivo.", nameof(f.IdEmpleado));
        }

        var nombre = f.NombreUsuario.Trim();
        // El índice único de usuario incluye eliminados.
        if (await _db.Usuario.AnyAsync(u => u.Usuario1 == nombre && u.Id != idExcluido, cancellationToken))
        {
            return OperationResult.Fail("Ese nombre de usuario ya está en uso.", nameof(f.NombreUsuario));
        }

        return null;
    }

    /// <summary>Evita dejar el sistema sin ningún administrador activo (bloqueo total del panel).</summary>
    private Task<bool> QuedaOtroAdministradorActivoAsync(int idUsuarioAfectado, CancellationToken cancellationToken) =>
        _db.Usuario.AnyAsync(
            u => u.Id != idUsuarioAfectado
                 && u.Role == Roles.Administrador
                 && u.Estado == EstadoRegistro.Activo,
            cancellationToken);
}
