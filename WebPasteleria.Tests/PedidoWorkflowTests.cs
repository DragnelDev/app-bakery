using WebPasteleria.Domain;

namespace WebPasteleria.Tests;

public class PedidoWorkflowTests
{
    [Theory]
    [InlineData(PedidoEstado.Pendiente, PedidoEstado.Confirmado)]
    [InlineData(PedidoEstado.Confirmado, PedidoEstado.EnPreparacion)]
    [InlineData(PedidoEstado.EnPreparacion, PedidoEstado.ListoParaEntrega)]
    [InlineData(PedidoEstado.ListoParaEntrega, PedidoEstado.Entregado)]
    [InlineData(PedidoEstado.EnCamino, PedidoEstado.Entregado)]
    public void Permite_avanzar_un_paso(byte actual, byte nuevo) =>
        Assert.True(PedidoWorkflow.PuedeTransicionar(actual, nuevo));

    [Theory]
    [InlineData(PedidoEstado.Pendiente, PedidoEstado.Entregado)]
    [InlineData(PedidoEstado.Pendiente, PedidoEstado.EnCamino)]
    [InlineData(PedidoEstado.Confirmado, PedidoEstado.Entregado)]
    public void No_permite_saltar_estados(byte actual, byte nuevo) =>
        Assert.False(PedidoWorkflow.PuedeTransicionar(actual, nuevo));

    [Theory]
    [InlineData(PedidoEstado.Entregado)]
    [InlineData(PedidoEstado.Cancelado)]
    public void Los_estados_finales_no_tienen_salida(byte final)
    {
        Assert.Empty(PedidoWorkflow.EstadosSiguientes(final));
        Assert.True(PedidoEstado.EsFinal(final));
    }

    [Theory]
    [InlineData(PedidoEstado.Pendiente)]
    [InlineData(PedidoEstado.Confirmado)]
    [InlineData(PedidoEstado.EnPreparacion)]
    [InlineData(PedidoEstado.ListoParaEntrega)]
    [InlineData(PedidoEstado.EnCamino)]
    public void Cualquier_estado_no_final_se_puede_cancelar(byte actual) =>
        Assert.True(PedidoWorkflow.PuedeTransicionar(actual, PedidoEstado.Cancelado));

    [Fact]
    public void Todos_los_estados_tienen_descripcion() =>
        Assert.All(PedidoEstado.Todos, e => Assert.False(string.IsNullOrWhiteSpace(e.Nombre)));
}
