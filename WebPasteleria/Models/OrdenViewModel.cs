using System;
using System.Collections.Generic;

namespace WebPasteleria.Models;

public class OrdenViewModel
{
    public int Id { get; set; }
    public string NombreCliente { get; set; }
    public string Producto { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } // Pendiente, Preparacion, Entregado
}
