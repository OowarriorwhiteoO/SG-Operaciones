using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestion.Infrastructure.Identity;
using SistemaGestion.Infrastructure.Persistence;
using SistemaGestion.Web.Models;

namespace SistemaGestion.Web.Controllers;

[Authorize(Roles = "Administrador")]
public sealed class UsuariosController(UserManager<ApplicationUser> users, RoleManager<IdentityRole> roles) : Controller
{
    public async Task<IActionResult> Index()
    {
        var result = new List<UsuarioListadoViewModel>();
        foreach (var user in await users.Users.AsNoTracking().OrderBy(x => x.Email).ToListAsync())
            result.Add(new(user.Id, user.Email ?? "", user.NombreCompleto, user.Activo, string.Join(", ", await users.GetRolesAsync(user))));
        return View(result);
    }

    public IActionResult Crear() => View(new UsuarioCrearViewModel { RolesDisponibles = SeedData.Roles });

    [HttpPost]
    public async Task<IActionResult> Crear(UsuarioCrearViewModel model)
    {
        model.RolesDisponibles = SeedData.Roles;
        if (!ModelState.IsValid) return View(model);
        if (!await roles.RoleExistsAsync(model.Rol)) { ModelState.AddModelError(nameof(model.Rol), "El rol seleccionado no existe."); return View(model); }
        var user = new ApplicationUser { UserName = model.Email.Trim(), Email = model.Email.Trim(), NombreCompleto = model.NombreCompleto.Trim(), EmailConfirmed = true, LockoutEnabled = true };
        var created = await users.CreateAsync(user, model.Password);
        if (!created.Succeeded) { foreach (var error in created.Errors) ModelState.AddModelError("", error.Description); return View(model); }
        var assigned = await users.AddToRoleAsync(user, model.Rol);
        if (!assigned.Succeeded) { await users.DeleteAsync(user); ModelState.AddModelError("", "No fue posible asignar el rol."); return View(model); }
        TempData["Mensaje"] = "Usuario creado y rol asignado."; return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> CambiarEstado(string id)
    {
        var user = await users.FindByIdAsync(id);
        if (user is null) return NotFound();
        if (users.GetUserId(User) == id && user.Activo) { TempData["Error"] = "No puede desactivar su propia cuenta."; return RedirectToAction(nameof(Index)); }
        user.Activo = !user.Activo;
        user.LockoutEnabled = true;
        user.LockoutEnd = user.Activo ? null : DateTimeOffset.MaxValue;
        await users.UpdateAsync(user);
        return RedirectToAction(nameof(Index));
    }
}
