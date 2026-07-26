using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaGestion.Application.Abstractions;
using SistemaGestion.Application.DTOs;

namespace SistemaGestion.Web.Controllers;

/// <summary>
/// Centraliza las opciones que solo puede modificar un administrador.
/// </summary>
[Authorize(Roles = "Administrador")]
public sealed class AdministracionController(IComercialService comercial) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct) =>
        View(await comercial.ObtenerDashboardAdministracionAsync(ct));

    [HttpGet]
    public async Task<IActionResult> Empresa(CancellationToken ct) =>
        View(await comercial.ObtenerEmpresaAsync(ct));

    [HttpPost]
    public async Task<IActionResult> Empresa(EmpresaInput input, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(input);
        var resultado = await comercial.GuardarEmpresaAsync(input, ct);
        if (!resultado.Exitoso)
        {
            ModelState.AddModelError("", resultado.Error!);
            return View(input);
        }
        TempData["Mensaje"] = "Los datos de la empresa fueron actualizados.";
        return RedirectToAction(nameof(Index));
    }
}

