using WebPasteleria.Common;

namespace WebPasteleria.Services.Security;

public sealed record AuthenticatedUser(int Id, string UserName, string FullName, string Email, string Role);

public interface IAuthService
{
    Task<OperationResult<AuthenticatedUser>> AutenticarAsync(
        string usuario, string clave, CancellationToken cancellationToken = default);
}
