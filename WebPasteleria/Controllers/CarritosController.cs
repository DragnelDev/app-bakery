using Microsoft.AspNetCore.Mvc;

namespace WebPasteleria.Controllers;

/// <summary>
/// Página del carrito. El contenido del carrito vive en el navegador (localStorage) hasta el checkout,
/// por lo que esta acción no consulta datos: los precios se vuelven a validar en el servidor al crear el pedido.
/// </summary>
public class CarritosController : Controller
{
    public IActionResult Index() => View();
}
