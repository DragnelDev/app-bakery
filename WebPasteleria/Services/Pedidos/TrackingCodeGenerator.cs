using System.Security.Cryptography;

namespace WebPasteleria.Services.Pedidos;

/// <summary>Genera códigos de seguimiento aleatorios y criptográficamente impredecibles.</summary>
public sealed class TrackingCodeGenerator : ITrackingCodeGenerator
{
    // Sin 0/O ni 1/I para evitar confusiones al dictar o transcribir el código.
    private const string Alfabeto = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private const int Longitud = 8;

    public string Generar()
    {
        var caracteres = new char[Longitud];
        for (var i = 0; i < caracteres.Length; i++)
        {
            caracteres[i] = Alfabeto[RandomNumberGenerator.GetInt32(Alfabeto.Length)];
        }

        return new string(caracteres);
    }
}
