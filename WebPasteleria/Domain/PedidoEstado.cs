namespace WebPasteleria.Domain;

/// <summary>Datos de presentación asociados a un estado de pedido.</summary>
public sealed record PedidoEstadoInfo(
    byte Codigo,
    string Nombre,
    string Icono,
    string Color,
    string Descripcion,
    int Progreso);

/// <summary>
/// Única fuente de verdad de los estados de un pedido (códigos y metadatos).
/// Agregar un estado nuevo implica agregar una constante y una fila en <see cref="Catalogo"/>.
/// </summary>
public static class PedidoEstado
{
    public const byte Pendiente = 0;
    public const byte Confirmado = 1;
    public const byte EnPreparacion = 2;
    public const byte ListoParaEntrega = 3;
    public const byte EnCamino = 4;
    public const byte Entregado = 5;
    public const byte Cancelado = 6;

    private static readonly PedidoEstadoInfo Desconocido =
        new(255, "Desconocido", "fa-circle", "#6c757d", "Estado desconocido", 0);

    private static readonly IReadOnlyDictionary<byte, PedidoEstadoInfo> Catalogo =
        new[]
        {
            new PedidoEstadoInfo(Pendiente, "Pendiente", "fa-clock-o", "#ffc107",
                "Tu pedido ha sido recibido y está pendiente de confirmación", 10),
            new PedidoEstadoInfo(Confirmado, "Confirmado", "fa-check", "#17a2b8",
                "Tu pedido ha sido confirmado y pronto comenzaremos a prepararlo", 30),
            new PedidoEstadoInfo(EnPreparacion, "En Preparación", "fa-cutlery", "#ff91c7",
                "Estamos preparando tu pedido con los mejores ingredientes", 50),
            new PedidoEstadoInfo(ListoParaEntrega, "Listo para Entrega", "fa-gift", "#28a745",
                "Tu pedido está listo para recogerse o para ser entregado", 70),
            new PedidoEstadoInfo(EnCamino, "En Camino", "fa-truck", "#007bff",
                "Tu pedido está en camino hacia tu dirección", 90),
            new PedidoEstadoInfo(Entregado, "Entregado", "fa-smile-o", "#28a745",
                "Tu pedido ha sido entregado exitosamente. ¡Disfrútalo!", 100),
            new PedidoEstadoInfo(Cancelado, "Cancelado", "fa-times", "#dc3545",
                "Tu pedido ha sido cancelado", 100),
        }.ToDictionary(e => e.Codigo);

    public static IReadOnlyCollection<PedidoEstadoInfo> Todos => Catalogo.Values.ToList();

    public static PedidoEstadoInfo Describir(byte codigo) =>
        Catalogo.TryGetValue(codigo, out var info) ? info : Desconocido;

    public static bool EsFinal(byte codigo) => codigo is Entregado or Cancelado;
}
