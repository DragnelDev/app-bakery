using System;
using System.Collections.Generic;

namespace WebPasteleria.Models;

public partial class Cliente
{
    public int Id { get; set; }

    public string CedulaIdentidad { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string Apellido { get; set; } = null!;

    public string? Celular { get; set; }

    public string? Email { get; set; }

    public string? Direccion { get; set; }

    public string? TipoCliente { get; set; }

    public string? Preferencias { get; set; }

    public string? UsuarioRegistro { get; set; }

    public DateTime FechaRegistro { get; set; }

    public byte Estado { get; set; }

    public bool? EsInvitado { get; set; }

    public virtual ICollection<Carrito> Carrito { get; set; } = new List<Carrito>();

    public virtual ICollection<Pedido> Pedido { get; set; } = new List<Pedido>();
}
