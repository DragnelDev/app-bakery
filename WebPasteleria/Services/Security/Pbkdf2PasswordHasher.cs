using System.Security.Cryptography;

namespace WebPasteleria.Services.Security;

/// <summary>
/// Hash de contraseñas con PBKDF2-HMAC-SHA256, sal aleatoria por contraseña y comparación en tiempo constante.
/// Formato almacenado: <c>PBKDF2$SHA256$iteraciones$salBase64$hashBase64</c>.
/// </summary>
public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const string Prefix = "PBKDF2";
    private const string Algorithm = "SHA256";
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int CurrentIterations = 600_000;
    private const int MaxAcceptedIterations = 5_000_000;

    private readonly ILegacyPasswordVerifier _legacyVerifier;

    public Pbkdf2PasswordHasher(ILegacyPasswordVerifier legacyVerifier)
    {
        _legacyVerifier = legacyVerifier;
    }

    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrEmpty(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, CurrentIterations, HashAlgorithmName.SHA256, HashSize);

        return $"{Prefix}${Algorithm}${CurrentIterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public PasswordVerificationOutcome Verify(string storedHash, string password)
    {
        if (string.IsNullOrEmpty(storedHash) || string.IsNullOrEmpty(password))
        {
            return PasswordVerificationOutcome.Failed;
        }

        if (!storedHash.StartsWith(Prefix + "$", StringComparison.Ordinal))
        {
            return _legacyVerifier.Matches(storedHash, password)
                ? PasswordVerificationOutcome.SuccessRehashNeeded
                : PasswordVerificationOutcome.Failed;
        }

        return VerifyCurrentFormat(storedHash, password);
    }

    private static PasswordVerificationOutcome VerifyCurrentFormat(string storedHash, string password)
    {
        var parts = storedHash.Split('$');
        if (parts.Length != 5
            || parts[1] != Algorithm
            || !int.TryParse(parts[2], out var iterations)
            || iterations <= 0
            || iterations > MaxAcceptedIterations)
        {
            return PasswordVerificationOutcome.Failed;
        }

        byte[] salt;
        byte[] expected;
        try
        {
            salt = Convert.FromBase64String(parts[3]);
            expected = Convert.FromBase64String(parts[4]);
        }
        catch (FormatException)
        {
            return PasswordVerificationOutcome.Failed;
        }

        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
        if (!CryptographicOperations.FixedTimeEquals(actual, expected))
        {
            return PasswordVerificationOutcome.Failed;
        }

        return iterations < CurrentIterations
            ? PasswordVerificationOutcome.SuccessRehashNeeded
            : PasswordVerificationOutcome.Success;
    }
}
