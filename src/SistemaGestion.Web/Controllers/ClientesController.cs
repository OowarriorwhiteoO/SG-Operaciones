using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaGestion.Application.Abstractions;
using SistemaGestion.Application.DTOs;

namespace SistemaGestion.Web.Controllers;

[Authorize(Policy = "GestionComercial")]
public sealed class ClientesController(IComercialService comercial) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct) => View(await comercial.ListarClientesAsync(ct));
    public IActionResult Crear() => View("Formulario", new ClienteInput());

    public async Task<IActionResult> Editar(int id, CancellationToken ct)
    {
        var item = await comercial.ObtenerClienteAsync(id, ct);
        return item is null ? NotFound() : View("Formulario", item);
    }

    [HttpPost]
    public async Task<IActionResult> Guardar(ClienteInput input, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View("Formulario", input);
        var resultado = await comercial.GuardarClienteAsync(input, ct);
        if (!resultado.Exitoso)
        {
            ModelState.AddModelError("", resultado.Error!);
            return View("Formulario", input);
        }
        TempData["Mensaje"] = "Cliente guardado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> CambiarEstado(int id, bool activar, CancellationToken ct)
    {
        var resultado = await comercial.CambiarEstadoClienteAsync(id, activar, ct);
        if (!resultado.Exitoso) TempData["Error"] = resultado.Error;
        return RedirectToAction(nameof(Index));
    }
}

