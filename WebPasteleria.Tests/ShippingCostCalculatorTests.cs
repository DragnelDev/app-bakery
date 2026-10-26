using WebPasteleria.Configuration;
using WebPasteleria.Services.Pedidos;

namespace WebPasteleria.Tests;

public class ShippingCostCalculatorTests
{
    private sealed class OpcionesFalsas<T> : Microsoft.Extensions.Options.IOptions<T> where T : class
    {
        public OpcionesFalsas(T valor) => Value = valor;
        public T Value { get; }
    }

    private static ShippingCostCalculator Crear() =>
        new(new OpcionesFalsas<EnvioOptions>(new EnvioOptions
        {
            TarifaPorDefecto = 25m,
            TarifasPorZona = new Dictionary<string, decimal> { ["Centro"] = 15m, ["Norte"] = 20m }
        }));

    [Fact]
    public void Recoger_en_tienda_no_cuesta() =>
        Assert.Equal(0m, Crear().Calcular("RECOGER", "Centro"));

    [Theory]
    [InlineData("Centro", 15)]
    [InlineData("centro", 15)]
    [InlineData(" Norte ", 20)]
    public void Domicilio_usa_la_tarifa_de_la_zona(string zona, int esperado) =>
        Assert.Equal(esperado, Crear().Calcular("DOMICILIO", zona));

    [Theory]
    [InlineData(null)]
    [InlineData("Zona inexistente")]
    public void Domicilio_sin_zona_conocida_usa_tarifa_por_defecto(string? zona) =>
        Assert.Equal(25m, Crear().Calcular("DOMICILIO", zona));
}
