using System;
using System.Collections.Generic;

namespace WebPasteleria.Models;

public partial class Pedido
{
    public int Id { get; set; }

    public string? CodigoSeguimiento { get; set; }

    public DateTime? FechaPedido { get; set; }

    public DateTime? FechaEntrega { get; set; }

    public int? IdCliente { get; set; }

    public string? NombreCliente { get; set; }

    public string? EmailCliente { get; set; }

    public string? CelularCliente { get; set; }

    public string? Direccion { get; set; }

    public int IdUsuario { get; set; }

    public string? TipoEntrega { get; set; }

    public int? MetodoPago { get; set; }

    public string? Observaciones { get; set; }

    public decimal? Total { get; set; }

    public byte? EstadoPedido { get; set; }

    public string UsuarioRegistro { get; set; } = null!;

    public DateTime FechaRegistro { get; set; }

    public byte Estado { get; set; }

    public virtual ICollection<DetallePedido> DetallePedido { get; set; } = new List<DetallePedido>();

    public virtual Cliente? IdClienteNavigation { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;

    public virtual MetodoPago? MetodoPagoNavigation { get; set; }

    public virtual ICollection<SeguimientoPedido> SeguimientoPedido { get; set; } = new List<SeguimientoPedido>();

    public virtual ICollection<TokenSeguimiento> TokenSeguimiento { get; set; } = new List<TokenSeguimiento>();
}
