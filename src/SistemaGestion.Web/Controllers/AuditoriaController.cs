using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaGestion.Application.Abstractions;
using SistemaGestion.Application.DTOs;
using SistemaGestion.Domain.Exceptions;

namespace SistemaGestion.Web.Controllers;

[Authorize(Policy = "ConsultarAuditoria")]
public sealed class AuditoriaController(IAuditoriaConsultaService auditoria) : Controller
{
    public async Task<IActionResult> Index([FromQuery] AuditoriaFiltro filtro, CancellationToken ct)
    {
        try { return View(await auditoria.ListarAsync(filtro, ct)); }
        catch (DomainException ex)
        {
            ModelState.AddModelError("", ex.Message);
            filtro.FechaDesde = filtro.FechaHasta = null;
            return View(await auditoria.ListarAsync(filtro, ct));
        }
    }
}
