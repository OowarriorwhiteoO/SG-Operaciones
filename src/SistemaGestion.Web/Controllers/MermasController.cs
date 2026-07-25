using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaGestion.Application.Abstractions;
using SistemaGestion.Application.DTOs;
using SistemaGestion.Domain.Enums;
using SistemaGestion.Domain.Exceptions;
using SistemaGestion.Web.Models;

namespace SistemaGestion.Web.Controllers;

[Authorize(Policy = "LecturaOperacional")]
public sealed class MermasController(
    IMermaService mermas,
    IEntradaService entradas,
    IMotivoMermaService motivos,
    ITipoRegistroService tipos,
    IExportacionService exportacion,
    IAuditoriaService auditoria) : Controller
{
    public async Task<IActionResult> Index([FromQuery] MermaFiltro filtro, CancellationToken ct)
    {
        try { return View(new MermaIndexViewModel(filtro, await mermas.ListarAsync(filtro, ct), await motivos.ListarAsync(ct))); }
        catch (DomainException ex)
        {
            ModelState.AddModelError("", ex.Message); filtro.FechaDesde = filtro.FechaHasta = null;
            return View(new MermaIndexViewModel(filtro, await mermas.ListarAsync(filtro, ct), await motivos.ListarAsync(ct)));
        }
    }

    [Authorize(Policy = "CrearMovimientos")]
    public async Task<IActionResult> Crear(int? entradaId, CancellationToken ct)
    {
        var model = await CrearModeloAsync(new MermaInput { EntradaId = entradaId ?? 0 }, ct);
        var opcion = model.Entradas.SingleOrDefault(x => x.Id == entradaId);
        if (opcion is not null) model.Input.EntradaRowVersion = Convert.ToBase64String(opcion.RowVersion);
        return View(model);
    }

    [HttpPost, Authorize(Policy = "CrearMovimientos")]
    public async Task<IActionResult> Crear([Bind(Prefix = "Input")] MermaInput input, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(await CrearModeloAsync(input, ct));
        var result = await mermas.CrearAsync(input, ct);
        if (!result.Exitoso) { ModelState.AddModelError("", result.Error!); return View(await CrearModeloAsync(input, ct)); }
        TempData["Mensaje"] = "Merma registrada y auditada correctamente.";
        return RedirectToAction(nameof(Index), new { entradaId = input.EntradaId });
    }

    [Authorize(Policy = "GenerarReportes")]
    public async Task<IActionResult> Indicadores([FromQuery] IndicadorMermaFiltro filtro, CancellationToken ct)
    {
        try { return View(new IndicadorMermaViewModel(await mermas.ObtenerIndicadoresAsync(filtro, ct), await tipos.ListarAsync(ct))); }
        catch (DomainException ex)
        {
            ModelState.AddModelError("", ex.Message); filtro.FechaDesde = DateTime.Today.AddMonths(-1); filtro.FechaHasta = DateTime.Today;
            return View(new IndicadorMermaViewModel(await mermas.ObtenerIndicadoresAsync(filtro, ct), await tipos.ListarAsync(ct)));
        }
    }

    [HttpGet, Authorize(Policy = "GenerarReportes")]
    public async Task<IActionResult> ExportarIndicadoresCsv([FromQuery] IndicadorMermaFiltro filtro, CancellationToken ct)
    {
        var archivo = exportacion.IndicadoresCsv(await mermas.ObtenerIndicadoresAsync(filtro, ct));
        await auditoria.RegistrarYGuardarAsync("ExportarCSV", "IndicadoresMerma", archivo.NombreArchivo, filtro, ct);
        return File(archivo.Contenido, archivo.TipoContenido, archivo.NombreArchivo);
    }

    [HttpGet, Authorize(Policy = "GenerarReportes")]
    public async Task<IActionResult> ExportarIndicadoresPdf([FromQuery] IndicadorMermaFiltro filtro, CancellationToken ct)
    {
        var archivo = exportacion.IndicadoresPdf(await mermas.ObtenerIndicadoresAsync(filtro, ct));
        await auditoria.RegistrarYGuardarAsync("ExportarPDF", "IndicadoresMerma", archivo.NombreArchivo, filtro, ct);
        return File(archivo.Contenido, archivo.TipoContenido, archivo.NombreArchivo);
    }

    private async Task<MermaCrearViewModel> CrearModeloAsync(MermaInput input, CancellationToken ct) =>
        new(input, await entradas.ListarDisponiblesAsync(ct),
            (await motivos.ListarAsync(ct)).Where(x => x.Estado == EstadoCatalogo.Activo).ToList());
}
