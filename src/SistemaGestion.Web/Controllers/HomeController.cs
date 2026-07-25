using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaGestion.Application.Abstractions;
using SistemaGestion.Application.DTOs;
using SistemaGestion.Web.Models;

namespace SistemaGestion.Web.Controllers;

[Authorize]
public sealed class HomeController(IReporteService reportes, ILogger<HomeController> logger) : Controller
{
    public async Task<IActionResult> Index([FromQuery] DashboardFiltro filtro, CancellationToken ct)
    {
        logger.LogInformation("Dashboard solicitado. CorrelationId={CorrelationId} Usuario={Usuario}",
            HttpContext.TraceIdentifier, User.Identity?.Name);
        return View(await reportes.ObtenerDashboardAsync(filtro, ct));
    }

    [AllowAnonymous, ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

    [AllowAnonymous]
    public IActionResult Estado(int codigo)
    {
        Response.StatusCode = codigo;
        return View("Error", new ErrorViewModel
        {
            RequestId = HttpContext.TraceIdentifier,
            Message = codigo == 404 ? "No encontramos la página solicitada." :
                codigo == 403 ? "No tiene permisos para realizar esta operación." :
                "No fue posible completar la solicitud."
        });
    }
}
