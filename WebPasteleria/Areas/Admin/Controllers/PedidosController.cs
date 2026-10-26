using Microsoft.AspNetCore.Mvc;
using WebPasteleria.Services.Pedidos;

namespace WebPasteleria.Areas.Admin.Controllers;

public class PedidosController : AdminControllerBase
{
    private readonly IPedidoConsultaAdmin _consulta;
    private readonly IPedidoEstadoService _estados;

    public PedidosController(IPedidoConsultaAdmin consulta, IPedidoEstadoService estados)
    {
        _consulta = consulta;
        _estados = estados;
    }

    public async Task<IActionResult> Index() =>
        View(await _consulta.ListarAsync(HttpContext.RequestAborted));

    public Task<IActionResult> Detalle(int id) => MostrarAsync(id, soloPedido: false);

    public Task<IActionResult> Imprimir(int id) => MostrarAsync(id, soloPedido: false);

    // Esta vista solo necesita la cabecera del pedido.
    [HttpGet]
    public Task<IActionResult> ActualizarEstado(int id) => MostrarAsync(id, soloPedido: true);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ActualizarEstado(int id, int estadoPedido, string? descripcion)
    {
        if (estadoPedido is < 0 or > byte.MaxValue)
        {
            NotificarError("El estado seleccionado no es válido.");
            return RedirectToAction(nameof(ActualizarEstado), new { id });
        }

        var resultado = await _estados.CambiarEstadoAsync(
            id, (byte)estadoPedido, descripcion, UsuarioActual, HttpContext.RequestAborted);

        if (!resultado.Succeeded)
        {
            NotificarError(resultado.Error!);
            return RedirectToAction(nameof(ActualizarEstado), new { id });
        }

        NotificarExito("Estado actualizado correctamente.");
        return RedirectToAction(nameof(Detalle), new { id });
    }

    /// <summary>Lo invoca el listado por AJAX (fetch), por eso responde JSON.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        var resultado = await _estados.CancelarAsync(id, UsuarioActual, HttpContext.RequestAborted);

        return resultado.Succeeded
            ? Json(new { success = true, mensaje = "Pedido cancelado correctamente." })
            : Json(new { success = false, mensaje = resultado.Error });
    }

    private async Task<IActionResult> MostrarAsync(int id, bool soloPedido)
    {
        var detalle = await _consulta.ObtenerPorIdAsync(id, HttpContext.RequestAborted);
        if (detalle is null)
        {
            return NotFound();
        }

        return soloPedido ? View(detalle.Pedido) : View(detalle);
    }
}
