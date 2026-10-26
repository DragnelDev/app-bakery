using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebPasteleria.Domain;
using WebPasteleria.Services.Administracion;
using WebPasteleria.ViewModels;

namespace WebPasteleria.Areas.Admin.Controllers;

[Authorize(Roles = Roles.Administrador)]
public class EmpleadosController : AdminControllerBase
{
    private readonly IEmpleadoAdminService _empleados;

    public EmpleadosController(IEmpleadoAdminService empleados)
    {
        _empleados = empleados;
    }

    public async Task<IActionResult> Index() =>
        View(await _empleados.ListarAsync(HttpContext.RequestAborted));

    public async Task<IActionResult> Search(string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return RedirectToAction(nameof(Index));
        }

        return View("Index", await _empleados.BuscarAsync(searchTerm, HttpContext.RequestAborted));
    }

    public async Task<IActionResult> Details(int id)
    {
        var empleado = await _empleados.ObtenerAsync(id, HttpContext.RequestAborted);
        return empleado is null ? NotFound() : View(empleado);
    }

    public IActionResult Create() => View(new EmpleadoFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EmpleadoFormViewModel formulario)
    {
        if (ModelState.IsValid)
        {
            var resultado = await _empleados.CrearAsync(formulario, UsuarioActual, HttpContext.RequestAborted);
            if (resultado.Succeeded)
            {
                NotificarExito("Empleado registrado exitosamente.");
                return RedirectToAction(nameof(Index));
            }

            AgregarError(resultado);
        }

        return View(formulario);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var formulario = await _empleados.ObtenerFormularioAsync(id, HttpContext.RequestAborted);
        return formulario is null ? NotFound() : View(formulario);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EmpleadoFormViewModel formulario)
    {
        if (id != formulario.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var resultado = await _empleados.ActualizarAsync(id, formulario, HttpContext.RequestAborted);
            if (resultado.Succeeded)
            {
                NotificarExito("Empleado actualizado exitosamente.");
                return RedirectToAction(nameof(Index));
            }

            AgregarError(resultado);
        }

        return View(formulario);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var empleado = await _empleados.ObtenerAsync(id, HttpContext.RequestAborted);
        return empleado is null ? NotFound() : View(empleado);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var resultado = await _empleados.EliminarAsync(id, HttpContext.RequestAborted);
        if (resultado.Succeeded)
        {
            NotificarExito("Empleado eliminado exitosamente.");
        }
        else
        {
            NotificarError(resultado.Error!);
        }

        return RedirectToAction(nameof(Index));
    }
}
