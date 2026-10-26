using Microsoft.Extensions.Options;
using WebPasteleria.Configuration;
using WebPasteleria.Domain;

namespace WebPasteleria.Services.Pedidos;

/// <summary>Calcula el costo de envío según las tarifas por zona definidas en configuración.</summary>
public sealed class ShippingCostCalculator : IShippingCostCalculator
{
    private readonly decimal _tarifaPorDefecto;
    private readonly IReadOnlyDictionary<string, decimal> _tarifasPorZona;

    public ShippingCostCalculator(IOptions<EnvioOptions> options)
    {
        _tarifaPorDefecto = options.Value.TarifaPorDefecto;
        _tarifasPorZona = new Dictionary<string, decimal>(
            options.Value.TarifasPorZona, StringComparer.OrdinalIgnoreCase);
    }

    public decimal Calcular(string tipoEntrega, string? zona)
    {
        if (!TipoEntrega.EsDomicilio(tipoEntrega))
        {
            return 0m;
        }

        return zona is not null && _tarifasPorZona.TryGetValue(zona.Trim(), out var tarifa)
            ? tarifa
            : _tarifaPorDefecto;
    }
}
