namespace WebPasteleria.Services.Security;

public enum PasswordVerificationOutcome
{
    Failed,
    Success,

    /// <summary>La contraseña es correcta pero el hash almacenado usa un esquema antiguo y debe regenerarse.</summary>
    SuccessRehashNeeded
}

public interface IPasswordHasher
{
    string Hash(string password);

    PasswordVerificationOutcome Verify(string storedHash, string password);
}
