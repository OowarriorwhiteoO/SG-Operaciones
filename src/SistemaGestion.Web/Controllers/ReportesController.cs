using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaGestion.Application.Abstractions;
using SistemaGestion.Application.DTOs;
using SistemaGestion.Domain.Exceptions;
using SistemaGestion.Web.Models;

namespace SistemaGestion.Web.Controllers;

[Authorize(Policy = "GenerarReportes")]
public sealed class ReportesController(
    IReporteService reportes,
    IExportacionService exportacion,
    ITrabajadorService trabajadores,
    ITipoRegistroService tipos,
    IAuditoriaService auditoria) : Controller
{
    public async Task<IActionResult> Movimientos([FromQuery] ConsultaMovimientoFiltro filtro, CancellationToken ct)
    {
        try
        {
            return View(new ConsultaMovimientosViewModel(filtro,
                await reportes.ConsultarMovimientosAsync(filtro, ct),
                await trabajadores.ListarAsync(ct), await tipos.ListarAsync(ct)));
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError("", ex.Message);
            filtro.FechaDesde = filtro.FechaHasta = null;
            return View(new ConsultaMovimientosViewModel(filtro,
                await reportes.ConsultarMovimientosAsync(filtro, ct),
                await trabajadores.ListarAsync(ct), await tipos.ListarAsync(ct)));
        }
    }

    public async Task<IActionResult> Index([FromQuery] InformePeriodoFiltro filtro, CancellationToken ct)
    {
        try { return View(new InformeTiposViewModel(await reportes.ObtenerInformeTiposAsync(filtro, ct))); }
        catch (DomainException ex)
        {
            ModelState.AddModelError("", ex.Message);
            filtro.FechaDesde = DateTime.Today.AddMonths(-1); filtro.FechaHasta = DateTime.Today;
            return View(new InformeTiposViewModel(await reportes.ObtenerInformeTiposAsync(filtro, ct)));
        }
    }

    public async Task<IActionResult> Trabajador([FromQuery] InformeTrabajadorFiltro filtro, CancellationToken ct)
    {
        InformeTrabajadorDto? informe = null;
        if (filtro.TrabajadorId > 0)
        {
            try
            {
                informe = await reportes.ObtenerInformeTrabajadorAsync(filtro, ct);
                if (informe is null) ModelState.AddModelError("", "El trabajador seleccionado no existe.");
            }
            catch (DomainException ex) { ModelState.AddModelError("", ex.Message); }
        }
        return View(new InformeTrabajadorViewModel(filtro, informe, await trabajadores.ListarAsync(ct)));
    }

    [HttpGet]
    public async Task<IActionResult> ExportarTipos(string formato, [FromQuery] InformePeriodoFiltro filtro, CancellationToken ct)
    {
        var informe = await reportes.ObtenerInformeTiposAsync(filtro, ct);
        var archivo = formato.Equals("pdf", StringComparison.OrdinalIgnoreCase)
            ? exportacion.InformeTiposPdf(informe) : exportacion.InformeTiposCsv(informe);
        await auditoria.RegistrarYGuardarAsync($"Exportar{formato.ToUpperInvariant()}", "InformeTipos", archivo.NombreArchivo, filtro, ct);
        return File(archivo.Contenido, archivo.TipoContenido, archivo.NombreArchivo);
    }

    [HttpGet]
    public async Task<IActionResult> ExportarTrabajador(string formato, [FromQuery] InformeTrabajadorFiltro filtro, CancellationToken ct)
    {
        var informe = await reportes.ObtenerInformeTrabajadorAsync(filtro, ct);
        if (informe is null) return NotFound();
        var archivo = formato.Equals("pdf", StringComparison.OrdinalIgnoreCase)
            ? exportacion.InformeTrabajadorPdf(informe) : exportacion.InformeTrabajadorCsv(informe);
        await auditoria.RegistrarYGuardarAsync($"Exportar{formato.ToUpperInvariant()}", "InformeTrabajador", archivo.NombreArchivo, filtro, ct);
        return File(archivo.Contenido, archivo.TipoContenido, archivo.NombreArchivo);
    }
}
