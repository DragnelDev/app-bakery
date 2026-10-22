using System;
using System.Collections.Generic;

namespace WebPasteleria.Models;

public partial class Carrito
{
    public int Id { get; set; }

    public int? IdCliente { get; set; }

    public DateTime? Fecha { get; set; }

    public string UsuarioRegistro { get; set; } = null!;

    public virtual ICollection<CarritoDetalle> CarritoDetalle { get; set; } = new List<CarritoDetalle>();

    public virtual Cliente? IdClienteNavigation { get; set; }
}
