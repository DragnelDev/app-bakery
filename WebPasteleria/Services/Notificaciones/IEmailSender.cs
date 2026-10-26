using WebPasteleria.Common;

namespace WebPasteleria.Services.Notificaciones;

public interface IEmailSender
{
    /// <summary>Indica si hay un servidor SMTP configurado.</summary>
    bool EstaDisponible { get; }

    Task<OperationResult> EnviarAsync(
        string destinatario, string asunto, string cuerpo, string? responderA = null,
        CancellationToken cancellationToken = default);
}
