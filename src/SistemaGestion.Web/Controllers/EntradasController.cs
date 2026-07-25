using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaGestion.Application.Abstractions;
using SistemaGestion.Application.DTOs;
using SistemaGestion.Domain.Enums;
using SistemaGestion.Domain.Exceptions;
using SistemaGestion.Web.Models;

namespace SistemaGestion.Web.Controllers;

[Authorize(Policy = "LecturaOperacional")]
public sealed class EntradasController(IEntradaService entradas, ITipoRegistroService tipos, ISaldoService saldo) : Controller
{
    public async Task<IActionResult> Index([FromQuery] EntradaFiltro filtro, CancellationToken ct)
    {
        try
        {
            var result = await entradas.ListarAsync(filtro, ct);
            return View(new EntradaIndexViewModel(filtro, result, await tipos.ListarAsync(ct)));
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError("", ex.Message);
            filtro.FechaDesde = filtro.FechaHasta = null;
            return View(new EntradaIndexViewModel(filtro, await entradas.ListarAsync(filtro, ct), await tipos.ListarAsync(ct)));
        }
    }

    public async Task<IActionResult> Detalle(int id, CancellationToken ct)
    {
        var item = await entradas.ObtenerDetalleAsync(id, ct);
        return item is null ? NotFound() : View(item);
    }

    [Authorize(Policy = "CrearMovimientos")]
    public async Task<IActionResult> Crear(CancellationToken ct) =>
        View(new EntradaCrearViewModel(new EntradaInput(), (await tipos.ListarAsync(ct)).Where(x => x.Estado == EstadoCatalogo.Activo).ToList()));

    [HttpPost, Authorize(Policy = "CrearMovimientos")]
    public async Task<IActionResult> Crear([Bind(Prefix = "Input")] EntradaInput input, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(new EntradaCrearViewModel(input, (await tipos.ListarAsync(ct)).Where(x => x.Estado == EstadoCatalogo.Activo).ToList()));
        var result = await entradas.CrearAsync(input, ct);
        if (!result.Exitoso)
        {
            ModelState.AddModelError("", result.Error!);
            return View(new EntradaCrearViewModel(input, (await tipos.ListarAsync(ct)).Where(x => x.Estado == EstadoCatalogo.Activo).ToList()));
        }
        TempData["Mensaje"] = "Entrada registrada y auditada correctamente.";
        return RedirectToAction(nameof(Detalle), new { id = result.Valor });
    }

    [HttpGet, Authorize(Policy = "LecturaOperacional")]
    public async Task<IActionResult> Saldo(int id, CancellationToken ct)
    {
        var result = await saldo.ObtenerAsync(id, ct);
        return result is null ? NotFound() : Json(new
        {
            result.EntradaId,
            result.CantidadInicial,
            result.TotalAsignado,
            result.TotalMerma,
            result.Disponible,
            rowVersion = Convert.ToBase64String(result.RowVersion)
        });
    }
}
