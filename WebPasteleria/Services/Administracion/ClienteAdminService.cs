using Microsoft.EntityFrameworkCore;
using WebPasteleria.Common;
using WebPasteleria.Domain;
using WebPasteleria.Models;
using WebPasteleria.ViewModels;

namespace WebPasteleria.Services.Administracion;

public sealed class ClienteAdminService : IClienteAdminService
{
    private readonly Models.BakeryContext _db;
    private readonly TimeProvider _clock;

    public ClienteAdminService(Models.BakeryContext db, TimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<List<Cliente>> ListarAsync(CancellationToken cancellationToken = default) =>
        await _db.Cliente
            .AsNoTracking()
            .Where(c => c.Estado != EstadoRegistro.Eliminado)
            .OrderByDescending(c => c.FechaRegistro)
            .ToListAsync(cancellationToken);

    public async Task<Cliente?> ObtenerAsync(int id, CancellationToken cancellationToken = default) =>
        await _db.Cliente
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && c.Estado != EstadoRegistro.Eliminado, cancellationToken);

    public async Task<ClienteFormViewModel?> ObtenerFormularioAsync(int id, CancellationToken cancellationToken = default)
    {
        var c = await ObtenerAsync(id, cancellationToken);
        return c is null
            ? null
            : new ClienteFormViewModel
            {
                Id = c.Id,
                CedulaIdentidad = c.CedulaIdentidad,
                Nombre = c.Nombre,
                Apellido = c.Apellido,
                Celular = c.Celular,
                Email = c.Email,
                Direccion = c.Direccion,
                TipoCliente = c.TipoCliente,
                Preferencias = c.Preferencias,
                Estado = c.Estado,
                UsuarioRegistro = c.UsuarioRegistro,
                FechaRegistro = c.FechaRegistro
            };
    }

    public async Task<OperationResult> CrearAsync(
        ClienteFormViewModel formulario, string usuario, CancellationToken cancellationToken = default)
    {
        var cedula = formulario.CedulaIdentidad.Trim();
        if (await CedulaEnUsoAsync(cedula, null, cancellationToken))
        {
            return OperationResult.Fail("Esta cédula de identidad ya está registrada.", nameof(formulario.CedulaIdentidad));
        }

        var cliente = new Cliente
        {
            UsuarioRegistro = usuario,
            FechaRegistro = _clock.GetLocalNow().DateTime,
            Estado = EstadoRegistro.Activo,
            EsInvitado = false
        };
        AplicarFormulario(cliente, formulario);

        _db.Cliente.Add(cliente);
        await _db.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok();
    }

    public async Task<OperationResult> ActualizarAsync(
        int id, ClienteFormViewModel formulario, CancellationToken cancellationToken = default)
    {
        var cliente = await _db.Cliente
            .FirstOrDefaultAsync(c => c.Id == id && c.Estado != EstadoRegistro.Eliminado, cancellationToken);
        if (cliente is null)
        {
            return OperationResult.Fail("Cliente no encontrado.");
        }

        if (await CedulaEnUsoAsync(formulario.CedulaIdentidad.Trim(), id, cancellationToken))
        {
            return OperationResult.Fail("Esta cédula de identidad ya está registrada.", nameof(formulario.CedulaIdentidad));
        }

        AplicarFormulario(cliente, formulario);
        cliente.Estado = formulario.Estado;

        await _db.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok();
    }

    public async Task<OperationResult> EliminarAsync(int id, CancellationToken cancellationToken = default)
    {
        var cliente = await _db.Cliente
            .FirstOrDefaultAsync(c => c.Id == id && c.Estado != EstadoRegistro.Eliminado, cancellationToken);
        if (cliente is null)
        {
            return OperationResult.Fail("Cliente no encontrado.");
        }

        cliente.Estado = EstadoRegistro.Eliminado;
        await _db.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok();
    }

    private static void AplicarFormulario(Cliente cliente, ClienteFormViewModel f)
    {
        cliente.CedulaIdentidad = f.CedulaIdentidad.Trim();
        cliente.Nombre = f.Nombre.Trim();
        cliente.Apellido = f.Apellido.Trim();
        cliente.Celular = f.Celular.LimpiarYTruncar(20);
        cliente.Email = f.Email.LimpiarYTruncar(100);
        cliente.Direccion = f.Direccion.LimpiarYTruncar(200);
        cliente.TipoCliente = f.TipoCliente.LimpiarYTruncar(50);
        cliente.Preferencias = f.Preferencias.LimpiarYTruncar(500);
    }

    // El índice único incluye registros eliminados, por eso no se filtra por estado.
    private Task<bool> CedulaEnUsoAsync(string cedula, int? idExcluido, CancellationToken cancellationToken) =>
        _db.Cliente.AnyAsync(c => c.CedulaIdentidad == cedula && c.Id != idExcluido, cancellationToken);
}
