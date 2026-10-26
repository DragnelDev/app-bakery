namespace WebPasteleria.Domain;

/// <summary>
/// Valores de la columna <c>estado</c> usada para la eliminación lógica en las tablas maestras.
/// </summary>
/// <remarks>
/// El DDL documenta "-1 = eliminado", pero la columna es TINYINT (0..255) y no admite negativos,
/// por eso el estado "eliminado" se persiste como 255.
/// </remarks>
public static class EstadoRegistro
{
    public const byte Inactivo = 0;
    public const byte Activo = 1;
    public const byte Eliminado = 255;
}
