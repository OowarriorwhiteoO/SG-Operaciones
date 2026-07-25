using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaGestion.Application.Abstractions;
using SistemaGestion.Application.DTOs;

namespace SistemaGestion.Web.Controllers;

[Authorize(Policy = "AdministrarCatalogos")]
public sealed class TiposRegistroController(ITipoRegistroService service) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct) => View(await service.ListarAsync(ct));
    public IActionResult Crear() => View("Formulario", new TipoRegistroInput());
    public async Task<IActionResult> Editar(int id, CancellationToken ct)
    {
        var x = await service.ObtenerAsync(id, ct);
        return x is null ? NotFound() : View("Formulario", new TipoRegistroInput { Id = x.Id, Nombre = x.Nombre, UnidadMedida = x.UnidadMedida });
    }
    [HttpPost]
    public async Task<IActionResult> Guardar(TipoRegistroInput input, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View("Formulario", input);
        var r = await service.GuardarAsync(input, ct);
        if (!r.Exitoso) { ModelState.AddModelError("", r.Error!); return View("Formulario", input); }
        TempData["Mensaje"] = "Tipo de registro guardado."; return RedirectToAction(nameof(Index));
    }
    [HttpPost]
    public async Task<IActionResult> CambiarEstado(int id, bool activar, CancellationToken ct) { await service.CambiarEstadoAsync(id, activar, ct); return RedirectToAction(nameof(Index)); }
}
