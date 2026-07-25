using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaGestion.Application.Abstractions;
using SistemaGestion.Application.DTOs;
using SistemaGestion.Domain.Enums;
using SistemaGestion.Domain.Exceptions;
using SistemaGestion.Web.Models;

namespace SistemaGestion.Web.Controllers;

[Authorize(Policy = "LecturaOperacional")]
public sealed class AsignacionesController(IAsignacionService asignaciones, IEntradaService entradas, ITrabajadorService trabajadores) : Controller
{
    public async Task<IActionResult> Index([FromQuery] AsignacionFiltro filtro, CancellationToken ct)
    {
        try
        {
            return View(new AsignacionIndexViewModel(filtro, await asignaciones.ListarAsync(filtro, ct), await trabajadores.ListarAsync(ct)));
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError("", ex.Message);
            filtro.FechaDesde = filtro.FechaHasta = null;
            return View(new AsignacionIndexViewModel(filtro, await asignaciones.ListarAsync(filtro, ct), await trabajadores.ListarAsync(ct)));
        }
    }

    [Authorize(Policy = "CrearMovimientos")]
    public async Task<IActionResult> Crear(int? entradaId, CancellationToken ct)
    {
        var model = await CrearModeloAsync(new AsignacionInput { EntradaId = entradaId ?? 0 }, ct);
        if (entradaId.HasValue)
        {
            var opcion = model.Entradas.SingleOrDefault(x => x.Id == entradaId);
            if (opcion is not null) model.Input.EntradaRowVersion = Convert.ToBase64String(opcion.RowVersion);
        }
        return View(model);
    }

    [HttpPost, Authorize(Policy = "CrearMovimientos")]
    public async Task<IActionResult> Crear([Bind(Prefix = "Input")] AsignacionInput input, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(await CrearModeloAsync(input, ct));
        var result = await asignaciones.CrearAsync(input, ct);
        if (!result.Exitoso)
        {
            ModelState.AddModelError("", result.Error!);
            return View(await CrearModeloAsync(input, ct));
        }
        TempData["Mensaje"] = "Asignación registrada y auditada correctamente.";
        return RedirectToAction(nameof(Index), new { entradaId = input.EntradaId });
    }

    private async Task<AsignacionCrearViewModel> CrearModeloAsync(AsignacionInput input, CancellationToken ct) =>
        new(input, await entradas.ListarDisponiblesAsync(ct),
            (await trabajadores.ListarAsync(ct)).Where(x => x.Estado == EstadoCatalogo.Activo).ToList());
}
