using System.ComponentModel.DataAnnotations;

namespace WebPasteleria.Services.Pedidos;

/// <summary>
/// Datos que envía el checkout. Los precios y totales NO forman parte de la solicitud:
/// el servidor los calcula a partir del catálogo para que el cliente no pueda manipularlos.
/// </summary>
public sealed class CrearPedidoRequest
{
    [Required(ErrorMessage = "El nombre del cliente es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    public string NombreCliente { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
    [StringLength(100, ErrorMessage = "El correo no puede superar los 100 caracteres.")]
    public string EmailCliente { get; set; } = string.Empty;

    [Required(ErrorMessage = "El celular es obligatorio.")]
    [StringLength(20, ErrorMessage = "El celular no puede superar los 20 caracteres.")]
    public string CelularCliente { get; set; } = string.Empty;

    [StringLength(12, ErrorMessage = "La cédula no puede superar los 12 caracteres.")]
    public string? CedulaIdentidad { get; set; }

    [Required(ErrorMessage = "El tipo de entrega es obligatorio.")]
    public string TipoEntrega { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "La dirección no puede superar los 200 caracteres.")]
    public string? Direccion { get; set; }

    [StringLength(50)]
    public string? ZonaEntrega { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un método de pago.")]
    public int MetodoPago { get; set; }

    [StringLength(500, ErrorMessage = "Las observaciones no pueden superar los 500 caracteres.")]
    public string? Observaciones { get; set; }

    [MinLength(1, ErrorMessage = "El pedido debe contener al menos un producto.")]
    public List<ItemPedidoRequest> Productos { get; set; } = new();
}

public sealed class ItemPedidoRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Producto inválido.")]
    public int IdProducto { get; set; }

    [Range(1, 100, ErrorMessage = "La cantidad debe estar entre 1 y 100.")]
    public int Cantidad { get; set; }
}
