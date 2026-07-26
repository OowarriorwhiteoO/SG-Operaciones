using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaGestion.Application.Abstractions;
using SistemaGestion.Application.DTOs;

namespace SistemaGestion.Web.Controllers;

[Authorize(Policy = "GestionComercial")]
public sealed class ProductosController(IComercialService comercial) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct) => View(await comercial.ListarProductosAsync(ct));
    public IActionResult Crear() => View("Formulario", new ProductoServicioInput());

    public async Task<IActionResult> Editar(int id, CancellationToken ct)
    {
        var item = await comercial.ObtenerProductoAsync(id, ct);
        return item is null ? NotFound() : View("Formulario", item);
    }

    [HttpPost]
    public async Task<IActionResult> Guardar(ProductoServicioInput input, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View("Formulario", input);
        var resultado = await comercial.GuardarProductoAsync(input, ct);
        if (!resultado.Exitoso)
        {
            ModelState.AddModelError("", resultado.Error!);
            return View("Formulario", input);
        }
        TempData["Mensaje"] = "Producto o servicio guardado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> CambiarEstado(int id, bool activar, CancellationToken ct)
    {
        var resultado = await comercial.CambiarEstadoProductoAsync(id, activar, ct);
        if (!resultado.Exitoso) TempData["Error"] = resultado.Error;
        return RedirectToAction(nameof(Index));
    }
}

