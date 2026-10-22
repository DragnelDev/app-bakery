using System;
using System.Collections.Generic;

namespace WebPasteleria.Models;

public partial class Empleado
{
    public int Id { get; set; }

    public string CedulaIdentidad { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string ApellidoPaterno { get; set; } = null!;

    public string? ApellidoMaterno { get; set; }

    public DateOnly FechaNacimiento { get; set; }

    public string Direccion { get; set; } = null!;

    public string Celular { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Cargo { get; set; } = null!;

    public string UsuarioRegistro { get; set; } = null!;

    public DateTime FechaRegistro { get; set; }

    public byte Estado { get; set; }

    public virtual ICollection<Usuario> Usuario { get; set; } = new List<Usuario>();
}
