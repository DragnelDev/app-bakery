namespace WebPasteleria.Services.Security;

/// <summary>
/// Verifica hashes del esquema anterior (AES reversible). Existe únicamente para migrar
/// las cuentas existentes: tras el primer login correcto el hash se reemplaza por PBKDF2.
/// </summary>
public interface ILegacyPasswordVerifier
{
    bool Matches(string storedHash, string password);
}
