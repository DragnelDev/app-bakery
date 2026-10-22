using System;
using System.Collections.Generic;

namespace WebPasteleria.Models;

public partial class SeguimientoPedido
{
    public int Id { get; set; }

    public int IdPedido { get; set; }

    public byte EstadoPedido { get; set; }

    public DateTime? Fecha { get; set; }

    public string? Descripcion { get; set; }

    public string UsuarioRegistro { get; set; } = null!;

    public string? NotasInternas { get; set; }

    public virtual Pedido IdPedidoNavigation { get; set; } = null!;
}
