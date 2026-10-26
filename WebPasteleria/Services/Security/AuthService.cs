using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebPasteleria.Common;
using WebPasteleria.Domain;
using WebPasteleria.Models;
using WebPasteleria.Configuration;

namespace WebPasteleria.Services.Security;

public sealed class AuthService : IAuthService
{
    private const string MensajeCredencialesInvalidas = "Usuario o contraseña incorrectos.";
    private const string MensajeCuentaBloqueada =
        "La cuenta está bloqueada por demasiados intentos fallidos. Contacta a un administrador.";

    private readonly BakeryContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly SeguridadOptions _options;
    private readonly TimeProvider _clock;

    public AuthService(
        BakeryContext db,
        IPasswordHasher hasher,
        IOptions<SeguridadOptions> options,
        TimeProvider clock)
    {
        _db = db;
        _hasher = hasher;
        _options = options.Value;
        _clock = clock;
    }

    public async Task<OperationResult<AuthenticatedUser>> AutenticarAsync(
        string usuario, string clave, CancellationToken cancellationToken = default)
    {
        var cuenta = await _db.Usuario
            .Include(u => u.IdEmpleadoNavigation)
            .FirstOrDefaultAsync(u => u.Usuario1 == usuario && u.Estado == EstadoRegistro.Activo, cancellationToken);

        if (cuenta is null)
        {
            return OperationResult<AuthenticatedUser>.Fail(MensajeCredencialesInvalidas);
        }

        if ((cuenta.IntentosFallidos ?? 0) >= _options.MaxIntentosFallidos)
        {
            return OperationResult<AuthenticatedUser>.Fail(MensajeCuentaBloqueada);
        }

        var resultado = _hasher.Verify(cuenta.Clave, clave);
        if (resultado == PasswordVerificationOutcome.Failed)
        {
            cuenta.IntentosFallidos = (cuenta.IntentosFallidos ?? 0) + 1;
            await _db.SaveChangesAsync(cancellationToken);
            return OperationResult<AuthenticatedUser>.Fail(MensajeCredencialesInvalidas);
        }

        cuenta.IntentosFallidos = 0;
        cuenta.UltimoLogin = _clock.GetLocalNow().DateTime;
        if (resultado == PasswordVerificationOutcome.SuccessRehashNeeded)
        {
            cuenta.Clave = _hasher.Hash(clave);
        }

        await _db.SaveChangesAsync(cancellationToken);
        return OperationResult<AuthenticatedUser>.Ok(ToAuthenticatedUser(cuenta));
    }

    private static AuthenticatedUser ToAuthenticatedUser(Usuario cuenta)
    {
        var empleado = cuenta.IdEmpleadoNavigation;
        var nombreCompleto = $"{empleado.Nombre} {empleado.ApellidoPaterno} {empleado.ApellidoMaterno}".Trim();

        return new AuthenticatedUser(cuenta.Id, cuenta.Usuario1, nombreCompleto, empleado.Email, cuenta.Role);
    }
}
