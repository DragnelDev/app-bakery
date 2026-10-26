using Microsoft.AspNetCore.Mvc;
using WebPasteleria.Services.Administracion;
using WebPasteleria.ViewModels;

namespace WebPasteleria.Areas.Admin.Controllers;

public class CategoriasController : AdminControllerBase
{
    private readonly ICategoriaAdminService _categorias;

    public CategoriasController(ICategoriaAdminService categorias)
    {
        _categorias = categorias;
    }

    public async Task<IActionResult> Index() =>
        View(await _categorias.ListarAsync(HttpContext.RequestAborted));

    public async Task<IActionResult> Details(int id)
    {
        var categoria = await _categorias.ObtenerAsync(id, HttpContext.RequestAborted);
        return categoria is null ? NotFound() : View(categoria);
    }

    public IActionResult Create() => View(new CategoriaFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoriaFormViewModel formulario)
    {
        if (ModelState.IsValid)
        {
            var resultado = await _categorias.CrearAsync(formulario, UsuarioActual, HttpContext.RequestAborted);
            if (resultado.Succeeded)
            {
                NotificarExito("Categoría creada exitosamente.");
                return RedirectToAction(nameof(Index));
            }

            AgregarError(resultado);
        }

        return View(formulario);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var formulario = await _categorias.ObtenerFormularioAsync(id, HttpContext.RequestAborted);
        return formulario is null ? NotFound() : View(formulario);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CategoriaFormViewModel formulario)
    {
        if (id != formulario.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var resultado = await _categorias.ActualizarAsync(id, formulario, HttpContext.RequestAborted);
            if (resultado.Succeeded)
            {
                NotificarExito("Categoría actualizada exitosamente.");
                return RedirectToAction(nameof(Index));
            }

            AgregarError(resultado);
        }

        return View(formulario);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var categoria = await _categorias.ObtenerAsync(id, HttpContext.RequestAborted);
        return categoria is null ? NotFound() : View(categoria);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var resultado = await _categorias.EliminarAsync(id, HttpContext.RequestAborted);
        if (resultado.Succeeded)
        {
            NotificarExito("Categoría eliminada exitosamente.");
        }
        else
        {
            NotificarError(resultado.Error!);
        }

        return RedirectToAction(nameof(Index));
    }
}
