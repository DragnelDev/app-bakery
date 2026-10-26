namespace WebPasteleria.Domain;

/// <summary>Nombres de rol usados en los claims y en <c>[Authorize(Roles = ...)]</c>.</summary>
public static class Roles
{
    public const string Administrador = "Administrador";
    public const string Empleado = "Empleado";

    /// <summary>Cualquier miembro del personal con acceso al panel de administración.</summary>
    public const string Personal = Administrador + "," + Empleado;

    public static readonly IReadOnlyList<string> Asignables = new[] { Administrador, Empleado };
}
