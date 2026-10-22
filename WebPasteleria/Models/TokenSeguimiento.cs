using System;
using System.Collections.Generic;

namespace WebPasteleria.Models;

public partial class TokenSeguimiento
{
    public int Id { get; set; }

    public int IdPedido { get; set; }

    public string Token { get; set; } = null!;

    public DateTime FechaExpiracion { get; set; }

    public bool? Usado { get; set; }

    public virtual Pedido IdPedidoNavigation { get; set; } = null!;
}
