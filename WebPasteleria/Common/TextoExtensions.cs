namespace WebPasteleria.Common;

public static class TextoExtensions
{
    /// <summary>Recorta espacios y limita la longitud para respetar el tamaño de la columna. Devuelve null si queda vacío.</summary>
    public static string? LimpiarYTruncar(this string? valor, int longitudMaxima)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return null;
        }

        var limpio = valor.Trim();
        return limpio.Length <= longitudMaxima ? limpio : limpio[..longitudMaxima];
    }
}
