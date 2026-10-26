using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebPasteleria.Common;
using WebPasteleria.Domain;

namespace WebPasteleria.Areas.Admin.Controllers;

/// <summary>
/// Base de todos los controladores del panel. Centraliza la autorización (antes ningún controlador
/// del área tenía [Authorize], por lo que el panel era público) y las notificaciones al usuario.
/// </summary>
[Area("Admin")]
[Authorize(Roles = Roles.Personal)]
public abstract class AdminControllerBase : Controller
{
    protected string UsuarioActual => User.Identity?.Name ?? "Sistema";

    protected void NotificarExito(string mensaje) => Notificar("success", mensaje);

    protected void NotificarError(string mensaje) => Notificar("danger", mensaje);

    protected void NotificarAdvertencia(string mensaje) => Notificar("warning", mensaje);

    /// <summary>Traslada el error de una regla de negocio al ModelState (al campo indicado, o al formulario).</summary>
    protected void AgregarError(string? campo, string? mensaje) =>
        ModelState.AddModelError(campo ?? string.Empty, mensaje ?? "No se pudo completar la operación.");

    protected void AgregarError(OperationResult resultado) => AgregarError(resultado.Field, resultado.Error);

    private void Notificar(string tipo, string mensaje)
    {
        TempData["AlertType"] = tipo;
        TempData["AlertMessage"] = mensaje;
    }
}
