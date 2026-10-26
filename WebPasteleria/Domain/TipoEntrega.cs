namespace WebPasteleria.Domain;

public static class TipoEntrega
{
    public const string Recoger = "RECOGER";
    public const string Domicilio = "DOMICILIO";

    public static bool EsDomicilio(string? tipo) =>
        string.Equals(tipo, Domicilio, StringComparison.OrdinalIgnoreCase);

    public static bool EsValido(string? tipo) =>
        string.Equals(tipo, Recoger, StringComparison.OrdinalIgnoreCase) || EsDomicilio(tipo);
}
