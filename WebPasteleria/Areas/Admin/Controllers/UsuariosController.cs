using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebPasteleria.Domain;
using WebPasteleria.Services.Administracion;
using WebPasteleria.ViewModels;

namespace WebPasteleria.Areas.Admin.Controllers;

[Authorize(Roles = Roles.Administrador)]
public class UsuariosController : AdminControllerBase
{
    private readonly IUsuarioAdminService _usuarios;
    private readonly IEmpleadoLookup _empleados;

    public UsuariosController(IUsuarioAdminService usuarios, IEmpleadoLookup empleados)
    {
        _usuarios = usuarios;
        _empleados = empleados;
    }

    public async Task<IActionResult> Index() =>
        View(await _usuarios.ListarAsync(HttpContext.RequestAborted));

    public async Task<IActionResult> Details(int id)
    {
        var usuario = await _usuarios.ObtenerAsync(id, HttpContext.RequestAborted);
        return usuario is null ? NotFound() : View(usuario);
    }

    public async Task<IActionResult> Create() => await FormularioAsync(new UsuarioFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UsuarioFormViewModel formulario)
    {
        if (ModelState.IsValid)
        {
            var resultado = await _usuarios.CrearAsync(formulario, UsuarioActual, HttpContext.RequestAborted);
            if (resultado.Succeeded)
            {
                NotificarExito("Usuario creado exitosamente.");
                return RedirectToAction(nameof(Index));
            }

            AgregarError(resultado);
        }

        return await FormularioAsync(formulario);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var formulario = await _usuarios.ObtenerFormularioAsync(id, HttpContext.RequestAborted);
        return formulario is null ? NotFound() : await FormularioAsync(formulario);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UsuarioFormViewModel formulario)
    {
        if (id != formulario.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var resultado = await _usuarios.ActualizarAsync(id, formulario, HttpContext.RequestAborted);
            if (resultado.Succeeded)
            {
                NotificarExito("Usuario actualizado exitosamente.");
                return RedirectToAction(nameof(Index));
            }

            AgregarError(resultado);
        }

        return await FormularioAsync(formulario);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var usuario = await _usuarios.ObtenerAsync(id, HttpContext.RequestAborted);
        return usuario is null ? NotFound() : View(usuario);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (id.ToString() == User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
        {
            NotificarError("No puedes eliminar tu propio usuario.");
            return RedirectToAction(nameof(Index));
        }

        var resultado = await _usuarios.EliminarAsync(id, HttpContext.RequestAborted);
        if (resultado.Succeeded)
        {
            NotificarExito("Usuario eliminado exitosamente.");
        }
        else
        {
            NotificarError(resultado.Error!);
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult> FormularioAsync(UsuarioFormViewModel formulario)
    {
        var empleados = await _empleados.ListarActivosAsync(HttpContext.RequestAborted);
        ViewBag.Empleados = new SelectList(
            empleados.Select(e => new
            {
                e.Id,
                Texto = $"{e.Nombre} {e.ApellidoPaterno} ({e.Cargo})"
            }),
            "Id", "Texto", formulario.IdEmpleado);
        ViewBag.Roles = new SelectList(Roles.Asignables, formulario.Rol);
        return View(formulario);
    }
}
