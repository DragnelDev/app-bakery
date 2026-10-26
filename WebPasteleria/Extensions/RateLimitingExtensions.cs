using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace WebPasteleria.Extensions;

public static class PoliticasLimite
{
    /// <summary>Búsqueda/seguimiento de pedidos sin autenticación: evita enumerar códigos o correos.</summary>
    public const string ConsultaPublica = "consulta-publica";

    /// <summary>Intentos de inicio de sesión por IP (complementa el bloqueo por cuenta).</summary>
    public const string Login = "login";
}

public static class RateLimitingExtensions
{
    public static IServiceCollection AddPasteleriaRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy(PoliticasLimite.ConsultaPublica, contexto =>
                RateLimitPartition.GetFixedWindowLimiter(
                    ClaveCliente(contexto),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 20,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

            options.AddPolicy(PoliticasLimite.Login, contexto =>
                RateLimitPartition.GetFixedWindowLimiter(
                    ClaveCliente(contexto),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(5),
                        QueueLimit = 0
                    }));
        });

        return services;
    }

    // Detrás de un proxy inverso hay que habilitar ForwardedHeaders para que aquí llegue la IP real del cliente.
    private static string ClaveCliente(HttpContext contexto) =>
        contexto.Connection.RemoteIpAddress?.ToString() ?? "desconocido";
}
