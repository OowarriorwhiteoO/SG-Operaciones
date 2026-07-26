using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaGestion.Application.Abstractions;

namespace SistemaGestion.Web.Controllers;

[Authorize(Policy = "GestionComercial")]
public sealed class FacturasController(IComercialService comercial) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct) => View(await comercial.ListarFacturasAsync(ct));

    public async Task<IActionResult> Detalle(int id, CancellationToken ct)
    {
        var item = await comercial.ObtenerFacturaAsync(id, ct);
        return item is null ? NotFound() : View(item);
    }

    [HttpPost]
    public async Task<IActionResult> MarcarPagada(int id, DateTime fechaPago, string? referencia, CancellationToken ct)
    {
        var resultado = await comercial.MarcarFacturaPagadaAsync(id, fechaPago, referencia, ct);
        if (!resultado.Exitoso) TempData["Error"] = resultado.Error;
        else TempData["Mensaje"] = "Pago registrado correctamente.";
        return RedirectToAction(nameof(Detalle), new { id });
    }

    public async Task<IActionResult> Imprimir(int id, CancellationToken ct)
    {
        var item = await comercial.ObtenerFacturaAsync(id, ct);
        return item is null ? NotFound() : View(item);
    }
}
