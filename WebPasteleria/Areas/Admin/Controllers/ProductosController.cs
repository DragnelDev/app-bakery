using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebPasteleria.Domain;
using WebPasteleria.Services.Administracion;
using WebPasteleria.ViewModels;

namespace WebPasteleria.Areas.Admin.Controllers;

public class ProductosController : AdminControllerBase
{
    private readonly IProductoAdminService _productos;
    private readonly ICategoriaLookup _categorias;

    public ProductosController(IProductoAdminService productos, ICategoriaLookup categorias)
    {
        _productos = productos;
        _categorias = categorias;
    }

    public async Task<IActionResult> Index() =>
        View(await _productos.ListarAsync(HttpContext.RequestAborted));

    public async Task<IActionResult> Details(int id)
    {
        var producto = await _productos.ObtenerAsync(id, HttpContext.RequestAborted);
        return producto is null ? NotFound() : View(producto);
    }

    public async Task<IActionResult> Create() => await FormularioAsync(new ProductoFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductoFormViewModel formulario)
    {
        if (ModelState.IsValid)
        {
            var resultado = await _productos.CrearAsync(formulario, UsuarioActual, HttpContext.RequestAborted);
            if (resultado.Succeeded)
            {
                NotificarExito("Producto creado exitosamente.");
                return RedirectToAction(nameof(Index));
            }

            AgregarError(resultado);
        }

        return await FormularioAsync(formulario);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var formulario = await _productos.ObtenerFormularioAsync(id, HttpContext.RequestAborted);
        return formulario is null ? NotFound() : await FormularioAsync(formulario);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductoFormViewModel formulario)
    {
        if (id != formulario.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var resultado = await _productos.ActualizarAsync(id, formulario, HttpContext.RequestAborted);
            if (resultado.Succeeded)
            {
                NotificarExito("Producto actualizado exitosamente.");
                return RedirectToAction(nameof(Index));
            }

            AgregarError(resultado);
        }

        return await FormularioAsync(formulario);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var producto = await _productos.ObtenerAsync(id, HttpContext.RequestAborted);
        return producto is null ? NotFound() : View(producto);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var resultado = await _productos.EliminarAsync(id, HttpContext.RequestAborted);
        if (resultado.Succeeded)
        {
            NotificarExito("Producto eliminado exitosamente.");
        }
        else
        {
            NotificarError(resultado.Error!);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int id)
    {
        var resultado = await _productos.AlternarEstadoAsync(id, HttpContext.RequestAborted);
        if (!resultado.Succeeded)
        {
            return Json(new { success = false, mensaje = resultado.Error });
        }

        var activo = resultado.Value == EstadoRegistro.Activo;
        return Json(new
        {
            success = true,
            mensaje = activo ? "Producto activado" : "Producto desactivado",
            nuevoEstado = resultado.Value
        });
    }

    private async Task<IActionResult> FormularioAsync(ProductoFormViewModel formulario)
    {
        var categorias = await _categorias.ListarActivasAsync(HttpContext.RequestAborted);
        ViewData["IdCategoria"] = new SelectList(categorias, "Id", "Nombre", formulario.IdCategoria);
        return View(formulario);
    }
}
