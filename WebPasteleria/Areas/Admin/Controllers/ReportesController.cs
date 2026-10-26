using Microsoft.AspNetCore.Mvc;
using WebPasteleria.Services.Reportes;
using WebPasteleria.ViewModels;

namespace WebPasteleria.Areas.Admin.Controllers;

public class ReportesController : AdminControllerBase
{
    private const int DiasPorDefecto = 30;
    private const int TopPorDefecto = 10;

    private readonly IReporteService _reportes;
    private readonly TimeProvider _clock;

    public ReportesController(IReporteService reportes, TimeProvider clock)
    {
        _reportes = reportes;
        _clock = clock;
    }

    public async Task<IActionResult> Ventas(DateOnly? desde, DateOnly? hasta) =>
        View(await _reportes.VentasAsync(Rango(desde, hasta), HttpContext.RequestAborted));

    public async Task<IActionResult> ProductosMasVendidos(DateOnly? desde, DateOnly? hasta) =>
        View(await _reportes.ProductosMasVendidosAsync(Rango(desde, hasta), TopPorDefecto, HttpContext.RequestAborted));

    public async Task<IActionResult> ClientesFrecuentes(DateOnly? desde, DateOnly? hasta) =>
        View(await _reportes.ClientesFrecuentesAsync(Rango(desde, hasta), TopPorDefecto, HttpContext.RequestAborted));

    /// <summary>Por defecto los últimos 30 días. Si el rango llega invertido se corrige en lugar de fallar.</summary>
    private RangoFechasViewModel Rango(DateOnly? desde, DateOnly? hasta)
    {
        var fin = hasta ?? DateOnly.FromDateTime(_clock.GetLocalNow().DateTime);
        var inicio = desde ?? fin.AddDays(-DiasPorDefecto);

        return inicio <= fin ? new RangoFechasViewModel(inicio, fin) : new RangoFechasViewModel(fin, inicio);
    }
}
