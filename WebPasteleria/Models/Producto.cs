using System;
using System.Collections.Generic;

namespace WebPasteleria.Models;

public partial class Producto
{
    public int Id { get; set; }

    public string? Codigo { get; set; }

    public string Nombre { get; set; } = null!;

    public int IdCategoria { get; set; }

    public string? UnidadMedida { get; set; }

    public decimal Precio { get; set; }

    public int? Stock { get; set; }

    public string? Tipo { get; set; }

    public string? Descripcion { get; set; }

    public string? ImagenUrl { get; set; }

    public string UsuarioRegistro { get; set; } = null!;

    public DateTime FechaRegistro { get; set; }

    public byte Estado { get; set; }

    public virtual ICollection<CarritoDetalle> CarritoDetalle { get; set; } = new List<CarritoDetalle>();

    public virtual ICollection<DetallePedido> DetallePedido { get; set; } = new List<DetallePedido>();

    public virtual CategoriaProducto IdCategoriaNavigation { get; set; } = null!;
}
