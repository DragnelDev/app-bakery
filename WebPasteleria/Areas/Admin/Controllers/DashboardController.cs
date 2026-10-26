using Microsoft.AspNetCore.Mvc;
using WebPasteleria.Services.Reportes;

namespace WebPasteleria.Areas.Admin.Controllers;

public class DashboardController : AdminControllerBase
{
    private readonly IDashboardService _dashboard;

    public DashboardController(IDashboardService dashboard)
    {
        _dashboard = dashboard;
    }

    public async Task<IActionResult> Index() =>
        View(await _dashboard.ObtenerResumenAsync(HttpContext.RequestAborted));
}
