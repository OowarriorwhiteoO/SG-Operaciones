using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaGestion.Application.Abstractions;
using SistemaGestion.Application.DTOs;

namespace SistemaGestion.Web.Controllers;

[Authorize(Policy = "GestionarTrabajadores")]
public sealed class TrabajadoresController(ITrabajadorService service) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct) => View(await service.ListarAsync(ct));
    public IActionResult Crear() => View("Formulario", new TrabajadorInput());
    public async Task<IActionResult> Editar(int id, CancellationToken ct)
    {
        var item = await service.ObtenerAsync(id, ct);
        return item is null ? NotFound() : View("Formulario", new TrabajadorInput { Id = item.Id, Rut = item.Rut, NombreCompleto = item.NombreCompleto, Area = item.Area });
    }
    [HttpPost]
    public async Task<IActionResult> Guardar(TrabajadorInput input, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View("Formulario", input);
        var result = await service.GuardarAsync(input, ct);
        if (!result.Exitoso) { ModelState.AddModelError("", result.Error!); return View("Formulario", input); }
        TempData["Mensaje"] = "Trabajador guardado correctamente."; return RedirectToAction(nameof(Index));
    }
    [HttpPost]
    public async Task<IActionResult> CambiarEstado(int id, bool activar, CancellationToken ct)
    {
        var result = await service.CambiarEstadoAsync(id, activar, ct);
        if (!result.Exitoso) TempData["Error"] = result.Error;
        return RedirectToAction(nameof(Index));
    }
}
