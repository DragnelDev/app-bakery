namespace WebPasteleria.Services.Pedidos;

public interface IShippingCostCalculator
{
    decimal Calcular(string tipoEntrega, string? zona);
}
