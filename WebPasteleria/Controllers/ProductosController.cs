using Microsoft.AspNetCore.Mvc;
using WebPasteleria.Services.Catalogo;

namespace WebPasteleria.Controllers;

/// <summary>Catálogo público de productos.</summary>
public class ProductosController : Controller
{
    private readonly ICatalogoService _catalogo;

    public ProductosController(ICatalogoService catalogo)
    {
        _catalogo = catalogo;
    }

    public async Task<IActionResult> Index(string? categoria, string? buscar, string? ordenar)
    {
        var token = HttpContext.RequestAborted;
        var productos = await _catalogo.ListarAsync(new CatalogoFiltro(categoria, buscar, ordenar), token);

        ViewBag.Categorias = await _catalogo.ListarCategoriasAsync(token);
        ViewBag.CategoriaSeleccionada = categoria;
        ViewBag.TerminoBusqueda = buscar;

        return View(productos);
    }

    public async Task<IActionResult> Detalle(int id)
    {
        var detalle = await _catalogo.ObtenerDetalleAsync(id, HttpContext.RequestAborted);
        if (detalle is null)
        {
            return NotFound();
        }

        ViewBag.ProductosRelacionados = detalle.Relacionados;
        return View(detalle.Producto);
    }

    public IActionResult PorCategoria(string? categoria) =>
        RedirectToAction(nameof(Index), new { categoria });

    // ---- Endpoints JSON usados por el front ----

    [HttpGet]
    public async Task<IActionResult> ObtenerProducto(int id)
    {
        var detalle = await _catalogo.ObtenerDetalleAsync(id, HttpContext.RequestAborted);
        if (detalle is null)
        {
            return NotFound(new { mensaje = "Producto no encontrado" });
        }

        var p = detalle.Producto;
        return Json(new
        {
            id = p.Id,
            nombre = p.Nombre,
            codigo = p.Codigo,
            precio = p.Precio,
            descripcion = p.Descripcion ?? "Sin descripción",
            stock = p.Stock,
            imagenUrl = p.ImagenUrl ?? "/images/no-image.svg",
            categoria = p.IdCategoriaNavigation?.Nombre ?? "Sin categoría",
            unidadMedida = p.UnidadMedida,
            tipo = p.Tipo
        });
    }

    [HttpGet]
    public async Task<IActionResult> BuscarProductos(string? termino) =>
        Json(await _catalogo.BuscarAsync(termino ?? string.Empty, HttpContext.RequestAborted));

    [HttpGet]
    public async Task<IActionResult> VerificarStock(int id, int cantidad)
    {
        var disponibilidad = await _catalogo.VerificarStockAsync(id, cantidad, HttpContext.RequestAborted);
        return Json(new
        {
            disponible = disponibilidad.Disponible,
            stockDisponible = disponibilidad.StockDisponible,
            mensaje = disponibilidad.Mensaje
        });
    }
}
