namespace WebPasteleria.Domain;

/// <summary>
/// Reglas de transición entre estados de un pedido.
/// Es una clase pura (sin acceso a datos) para poder probarla de forma aislada.
/// </summary>
public static class PedidoWorkflow
{
    private static readonly IReadOnlyDictionary<byte, byte[]> Transiciones =
        new Dictionary<byte, byte[]>
        {
            [PedidoEstado.Pendiente] = new[] { PedidoEstado.Confirmado, PedidoEstado.Cancelado },
            [PedidoEstado.Confirmado] = new[] { PedidoEstado.Pendiente, PedidoEstado.EnPreparacion, PedidoEstado.Cancelado },
            [PedidoEstado.EnPreparacion] = new[] { PedidoEstado.Confirmado, PedidoEstado.ListoParaEntrega, PedidoEstado.Cancelado },
            // Un pedido para recoger pasa de "Listo" directamente a "Entregado".
            [PedidoEstado.ListoParaEntrega] = new[] { PedidoEstado.EnPreparacion, PedidoEstado.EnCamino, PedidoEstado.Entregado, PedidoEstado.Cancelado },
            [PedidoEstado.EnCamino] = new[] { PedidoEstado.ListoParaEntrega, PedidoEstado.Entregado, PedidoEstado.Cancelado },
            [PedidoEstado.Entregado] = Array.Empty<byte>(),
            [PedidoEstado.Cancelado] = Array.Empty<byte>(),
        };

    public static IReadOnlyList<byte> EstadosSiguientes(byte actual) =>
        Transiciones.TryGetValue(actual, out var siguientes) ? siguientes : Array.Empty<byte>();

    public static bool PuedeTransicionar(byte actual, byte nuevo) =>
        EstadosSiguientes(actual).Contains(nuevo);
}
