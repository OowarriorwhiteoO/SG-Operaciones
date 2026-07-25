using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaGestion.Application.Abstractions;
using SistemaGestion.Application.DTOs;
using SistemaGestion.Domain.Enums;
using SistemaGestion.Web.Models;

namespace SistemaGestion.Web.Controllers;

[Authorize(Policy = "AnularMovimientos")]
public sealed class AnulacionesController(IAnulacionService service) : Controller
{
    public async Task<IActionResult> Crear(ClaseMovimiento clase, int id, CancellationToken ct)
    {
        var detalle = await service.ObtenerAsync(clase, id, ct);
        return detalle is null ? NotFound() : View(new AnulacionViewModel(detalle,
            new AnulacionInput { Id = id, Clase = clase, RowVersion = Convert.ToBase64String(detalle.RowVersion) }));
    }

    [HttpPost]
    public async Task<IActionResult> Crear([Bind(Prefix = "Input")] AnulacionInput input, CancellationToken ct)
    {
        var detalle = await service.ObtenerAsync(input.Clase, input.Id, ct);
        if (detalle is null) return NotFound();
        if (!ModelState.IsValid) return View(new AnulacionViewModel(detalle, input));
        var result = await service.AnularAsync(input, ct);
        if (!result.Exitoso) { ModelState.AddModelError("", result.Error!); return View(new AnulacionViewModel(detalle, input)); }
        TempData["Mensaje"] = $"{input.Clase} anulada correctamente. El registro permanece en el historial.";
        return input.Clase switch
        {
            ClaseMovimiento.Entrada => RedirectToAction("Detalle", "Entradas", new { id = input.Id }),
            ClaseMovimiento.Asignacion => RedirectToAction("Index", "Asignaciones"),
            _ => RedirectToAction("Index", "Mermas")
        };
    }
}
