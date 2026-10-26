using Microsoft.AspNetCore.Mvc;
using WebPasteleria.Services.Administracion;
using WebPasteleria.ViewModels;

namespace WebPasteleria.Areas.Admin.Controllers;

public class ClientesController : AdminControllerBase
{
    private readonly IClienteAdminService _clientes;

    public ClientesController(IClienteAdminService clientes)
    {
        _clientes = clientes;
    }

    public async Task<IActionResult> Index() =>
        View(await _clientes.ListarAsync(HttpContext.RequestAborted));

    public async Task<IActionResult> Details(int id)
    {
        var cliente = await _clientes.ObtenerAsync(id, HttpContext.RequestAborted);
        return cliente is null ? NotFound() : View(cliente);
    }

    public IActionResult Create() => View(new ClienteFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClienteFormViewModel formulario)
    {
        if (ModelState.IsValid)
        {
            var resultado = await _clientes.CrearAsync(formulario, UsuarioActual, HttpContext.RequestAborted);
            if (resultado.Succeeded)
            {
                NotificarExito("Cliente registrado exitosamente.");
                return RedirectToAction(nameof(Index));
            }

            AgregarError(resultado);
        }

        return View(formulario);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var formulario = await _clientes.ObtenerFormularioAsync(id, HttpContext.RequestAborted);
        return formulario is null ? NotFound() : View(formulario);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ClienteFormViewModel formulario)
    {
        if (id != formulario.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var resultado = await _clientes.ActualizarAsync(id, formulario, HttpContext.RequestAborted);
            if (resultado.Succeeded)
            {
                NotificarExito("Cliente actualizado exitosamente.");
                return RedirectToAction(nameof(Index));
            }

            AgregarError(resultado);
        }

        return View(formulario);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var cliente = await _clientes.ObtenerAsync(id, HttpContext.RequestAborted);
        return cliente is null ? NotFound() : View(cliente);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var resultado = await _clientes.EliminarAsync(id, HttpContext.RequestAborted);
        if (resultado.Succeeded)
        {
            NotificarExito("Cliente eliminado exitosamente.");
        }
        else
        {
            NotificarError(resultado.Error!);
        }

        return RedirectToAction(nameof(Index));
    }
}
