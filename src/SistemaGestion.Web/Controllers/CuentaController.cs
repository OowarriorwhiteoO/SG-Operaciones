using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SistemaGestion.Infrastructure.Identity;
using SistemaGestion.Web.Models;
using Microsoft.AspNetCore.RateLimiting;

namespace SistemaGestion.Web.Controllers;

public sealed class CuentaController(SignInManager<ApplicationUser> signInManager) : Controller
{
    [AllowAnonymous] public IActionResult IniciarSesion() => View(new LoginViewModel());
    [HttpPost, AllowAnonymous, EnableRateLimiting("login")]
    public async Task<IActionResult> IniciarSesion(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(model);
        var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.Recordarme, lockoutOnFailure: true);
        if (result.Succeeded) return LocalRedirect(returnUrl ?? "/");
        ModelState.AddModelError("", result.IsLockedOut ? "Cuenta bloqueada temporalmente." : "Credenciales inválidas.");
        return View(model);
    }
    [HttpPost, Authorize]
    public async Task<IActionResult> CerrarSesion() { await signInManager.SignOutAsync(); return RedirectToAction(nameof(IniciarSesion)); }
    [AllowAnonymous] public IActionResult AccesoDenegado() => View();
}
