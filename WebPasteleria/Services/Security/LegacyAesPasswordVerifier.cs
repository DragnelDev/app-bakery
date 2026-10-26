using System.Security.Cryptography;
using System.Text;

namespace WebPasteleria.Services.Security;

/// <summary>
/// Réplica exacta del cifrado que usaba <c>AccountController.Encrypt</c>.
/// Se puede eliminar cuando todas las cuentas hayan iniciado sesión al menos una vez
/// (es decir, cuando ninguna clave almacenada carezca del prefijo "PBKDF2$").
/// </summary>
public sealed class LegacyAesPasswordVerifier : ILegacyPasswordVerifier
{
    private const string LegacyKey = "SIS457-1nf0!";
    private const int LegacyIterations = 1000;

    private static readonly byte[] LegacySalt =
    {
        0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76
    };

    public bool Matches(string storedHash, string password)
    {
        if (string.IsNullOrEmpty(storedHash) || string.IsNullOrEmpty(password))
        {
            return false;
        }

        // Históricamente la comparación se hacía en SQL Server con una collation que no distingue
        // mayúsculas (y el script de datos de ejemplo guarda el hash en mayúsculas), por eso aquí
        // también se ignora el caso. Solo aplica durante la ventana de migración.
        return string.Equals(Encrypt(password), storedHash, StringComparison.OrdinalIgnoreCase);
    }

    public static string Encrypt(string clearText)
    {
        // PBKDF2-SHA1 de 48 bytes == GetBytes(32) seguido de GetBytes(16) del Rfc2898DeriveBytes original.
        var derived = Rfc2898DeriveBytes.Pbkdf2(LegacyKey, LegacySalt, LegacyIterations, HashAlgorithmName.SHA1, 48);

        using var aes = Aes.Create();
        aes.Key = derived[..32];
        aes.IV = derived[32..48];

        var clearBytes = Encoding.Unicode.GetBytes(clearText);
        using var encryptor = aes.CreateEncryptor();
        var encrypted = encryptor.TransformFinalBlock(clearBytes, 0, clearBytes.Length);
        return Convert.ToBase64String(encrypted);
    }
}
