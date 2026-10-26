using WebPasteleria.Services.Security;

namespace WebPasteleria.Tests;

public class PasswordHasherTests
{
    // Hash de "hola123" tal como quedo guardado por el script SQL original (esquema AES anterior).
    private const string HashLegadoDelScriptSql = "I0HCOO/NSSY6WOS9POP5XW==";

    private static Pbkdf2PasswordHasher Crear() => new(new LegacyAesPasswordVerifier());

    [Fact]
    public void Hash_y_verificacion_correcta()
    {
        var hasher = Crear();
        var hash = hasher.Hash("Secreta#2026");

        Assert.StartsWith("PBKDF2$", hash);
        Assert.Equal(PasswordVerificationOutcome.Success, hasher.Verify(hash, "Secreta#2026"));
    }

    [Fact]
    public void Clave_incorrecta_falla() =>
        Assert.Equal(PasswordVerificationOutcome.Failed, Crear().Verify(Crear().Hash("Secreta#2026"), "otra"));

    [Fact]
    public void Dos_hashes_de_la_misma_clave_son_distintos() =>
        Assert.NotEqual(Crear().Hash("abc12345"), Crear().Hash("abc12345"));

    [Fact]
    public void Hash_legado_correcto_pide_migrar()
    {
        var resultado = Crear().Verify(HashLegadoDelScriptSql, "hola123");
        Assert.Equal(PasswordVerificationOutcome.SuccessRehashNeeded, resultado);
    }

    [Fact]
    public void Hash_legado_con_clave_incorrecta_falla() =>
        Assert.Equal(PasswordVerificationOutcome.Failed, Crear().Verify(HashLegadoDelScriptSql, "incorrecta"));

    [Theory]
    [InlineData("")]
    [InlineData("PBKDF2$SHA256$abc$xx$yy")]
    [InlineData("PBKDF2$SHA256$600000$no-es-base64$no-es-base64")]
    public void Hashes_mal_formados_fallan_sin_lanzar_excepcion(string almacenado) =>
        Assert.Equal(PasswordVerificationOutcome.Failed, Crear().Verify(almacenado, "hola123"));
}
