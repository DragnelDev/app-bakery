using WebPasteleria.Services.Pedidos;

namespace WebPasteleria.Tests;

public class TrackingCodeGeneratorTests
{
    [Fact]
    public void Genera_ocho_caracteres_sin_ambiguos()
    {
        var generador = new TrackingCodeGenerator();

        for (var i = 0; i < 200; i++)
        {
            var codigo = generador.Generar();
            Assert.Equal(8, codigo.Length);
            Assert.DoesNotContain(codigo, c => "01OI".Contains(c));
            Assert.All(codigo, c => Assert.True(char.IsUpper(c) || char.IsDigit(c)));
        }
    }

    [Fact]
    public void Los_codigos_no_se_repiten_en_una_muestra()
    {
        var generador = new TrackingCodeGenerator();
        var codigos = Enumerable.Range(0, 500).Select(_ => generador.Generar()).ToHashSet();
        Assert.Equal(500, codigos.Count);
    }
}
