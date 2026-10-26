namespace WebPasteleria.Configuration;

/// <summary>Tarifas de envío a domicilio. Se configuran en la sección "Envio" de appsettings.</summary>
public sealed class EnvioOptions
{
    public const string Seccion = "Envio";

    public decimal TarifaPorDefecto { get; set; } = 25m;

    public Dictionary<string, decimal> TarifasPorZona { get; set; } = new();
}

/// <summary>Parámetros del flujo de pedidos. Sección "Pedidos".</summary>
public sealed class PedidoOptions
{
    public const string Seccion = "Pedidos";

    /// <summary>Usuario del sistema al que se asignan los pedidos hechos desde la tienda en línea.</summary>
    public int UsuarioSistemaId { get; set; } = 1;

    public int DiasEntregaRecoger { get; set; } = 1;

    public int DiasEntregaDomicilio { get; set; } = 2;
}

/// <summary>Parámetros de seguridad de la autenticación. Sección "Seguridad".</summary>
public sealed class SeguridadOptions
{
    public const string Seccion = "Seguridad";

    public int MaxIntentosFallidos { get; set; } = 5;

    public int MinutosSesion { get; set; } = 30;
}

/// <summary>Configuración SMTP. Sección "Correo". Si <see cref="Host"/> está vacío el envío queda deshabilitado.</summary>
public sealed class CorreoOptions
{
    public const string Seccion = "Correo";

    public string? Host { get; set; }
    public int Puerto { get; set; } = 587;
    public bool UsarSsl { get; set; } = true;
    public string? Usuario { get; set; }
    public string? Clave { get; set; }
    public string? Remitente { get; set; }
    public string? DestinoContacto { get; set; }

    public bool EstaConfigurado => !string.IsNullOrWhiteSpace(Host) && !string.IsNullOrWhiteSpace(Remitente);
}
