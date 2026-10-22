using System;
using System.Collections.Generic;

namespace WebPasteleria.Models;

public partial class CategoriaProducto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public short Estado { get; set; }

    public virtual ICollection<Producto> Producto { get; set; } = new List<Producto>();
}
