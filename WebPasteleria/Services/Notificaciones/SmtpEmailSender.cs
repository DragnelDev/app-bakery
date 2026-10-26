using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using WebPasteleria.Common;
using WebPasteleria.Configuration;

namespace WebPasteleria.Services.Notificaciones;

/// <summary>Envía correos por SMTP. Si no hay configuración devuelve un error explícito en lugar de fingir el envío.</summary>
public sealed class SmtpEmailSender : IEmailSender
{
    private readonly CorreoOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<CorreoOptions> options, ILogger<SmtpEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public bool EstaDisponible => _options.EstaConfigurado;

    public async Task<OperationResult> EnviarAsync(
        string destinatario, string asunto, string cuerpo, string? responderA = null,
        CancellationToken cancellationToken = default)
    {
        if (!EstaDisponible)
        {
            return OperationResult.Fail("El envío de correos no está configurado.");
        }

        try
        {
            using var mensaje = new MailMessage(_options.Remitente!, destinatario, asunto, cuerpo);
            if (!string.IsNullOrWhiteSpace(responderA))
            {
                mensaje.ReplyToList.Add(new MailAddress(responderA));
            }

            using var cliente = new SmtpClient(_options.Host!, _options.Puerto) { EnableSsl = _options.UsarSsl };
            if (!string.IsNullOrWhiteSpace(_options.Usuario))
            {
                cliente.Credentials = new NetworkCredential(_options.Usuario, _options.Clave);
            }

            await cliente.SendMailAsync(mensaje, cancellationToken);
            return OperationResult.Ok();
        }
        catch (Exception ex) when (ex is SmtpException or FormatException or InvalidOperationException)
        {
            _logger.LogError(ex, "No se pudo enviar el correo a {Destinatario}", destinatario);
            return OperationResult.Fail("No se pudo enviar el correo. Intenta nuevamente más tarde.");
        }
    }
}
