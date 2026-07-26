using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaGestion.Application.Abstractions;
using SistemaGestion.Application.DTOs;
using SistemaGestion.Domain.Enums;
using SistemaGestion.Web.Models;

namespace SistemaGestion.Web.Controllers;

[Authorize(Policy = "GestionComercial")]
public sealed class CotizacionesController(IComercialService comercial) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct) => View(await comercial.ListarCotizacionesAsync(ct));

    [HttpGet]
    public async Task<IActionResult> Crear(CancellationToken ct) => View(await PrepararAsync(new CotizacionInput(), ct));

    [HttpPost]
    public async Task<IActionResult> Crear(CotizacionCrearViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(await PrepararAsync(model.Input, ct));
        var resultado = await comercial.CrearCotizacionAsync(model.Input, ct);
        if (!resultado.Exitoso)
        {
            ModelState.AddModelError("", resultado.Error!);
            return View(await PrepararAsync(model.Input, ct));
        }
        TempData["Mensaje"] = "Cotización creada en estado borrador.";
        return RedirectToAction(nameof(Detalle), new { id = resultado.Valor });
    }

    public async Task<IActionResult> Detalle(int id, CancellationToken ct)
    {
        var item = await comercial.ObtenerCotizacionAsync(id, ct);
        return item is null ? NotFound() : View(item);
    }

    [HttpPost]
    public async Task<IActionResult> CambiarEstado(int id, EstadoCotizacion estado, CancellationToken ct)
    {
        var resultado = await comercial.CambiarEstadoCotizacionAsync(id, estado, ct);
        if (!resultado.Exitoso) TempData["Error"] = resultado.Error;
        else TempData["Mensaje"] = $"Cotización actualizada a {estado}.";
        return RedirectToAction(nameof(Detalle), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Facturar(int id, CancellationToken ct)
    {
        var resultado = await comercial.FacturarCotizacionAsync(id, ct);
        if (!resultado.Exitoso)
        {
            TempData["Error"] = resultado.Error;
            return RedirectToAction(nameof(Detalle), new { id });
        }
        TempData["Mensaje"] = "Factura interna emitida correctamente.";
        return RedirectToAction("Detalle", "Facturas", new { id = resultado.Valor });
    }

    private async Task<CotizacionCrearViewModel> PrepararAsync(CotizacionInput input, CancellationToken ct) =>
        new()
        {
            Input = input,
            Clientes = (await comercial.ListarClientesAsync(ct)).Where(x => x.Estado == EstadoCatalogo.Activo).ToList(),
            Productos = (await comercial.ListarProductosAsync(ct)).Where(x => x.Estado == EstadoCatalogo.Activo).ToList()
        };
}

