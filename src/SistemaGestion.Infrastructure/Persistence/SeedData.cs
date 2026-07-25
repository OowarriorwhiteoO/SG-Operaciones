using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using SistemaGestion.Domain.Entities;
using SistemaGestion.Infrastructure.Identity;

namespace SistemaGestion.Infrastructure.Persistence;

public static class SeedData
{
    public static readonly string[] Roles = ["Administrador", "Bodega", "Supervisor", "Consulta"];

    public static async Task InitializeAsync(IServiceProvider services, CancellationToken ct = default)
    {
        // La inicialización es idempotente: aplica migraciones y completa únicamente datos ausentes.
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync(ct);
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in Roles)
            if (!await roles.RoleExistsAsync(role)) await roles.CreateAsync(new IdentityRole(role));

        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        // Las credenciales se leen desde configuración externa para mantenerlas fuera del código fuente.
        var password = Environment.GetEnvironmentVariable("SGW_ADMIN_PASSWORD")
            ?? configuration["SGW_ADMIN_PASSWORD"]
            ?? configuration["AdminSeed:Password"];
        var email = Environment.GetEnvironmentVariable("SGW_ADMIN_EMAIL")
            ?? configuration["SGW_ADMIN_EMAIL"]
            ?? configuration["AdminSeed:Email"];
        if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password))
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var admin = await users.FindByEmailAsync(email);
            if (admin is null)
            {
                admin = new ApplicationUser { UserName = email, Email = email, NombreCompleto = "Administrador", EmailConfirmed = true, LockoutEnabled = true };
                var result = await users.CreateAsync(admin, password);
                if (!result.Succeeded) throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));
            }
            if (!await users.IsInRoleAsync(admin, "Administrador")) await users.AddToRoleAsync(admin, "Administrador");
        }

        if (!await db.TiposRegistro.AnyAsync(ct))
            db.TiposRegistro.AddRange(new TipoRegistro("Elementos de protección personal", "unidad"), new TipoRegistro("Material de embalaje", "unidad"), new TipoRegistro("Materia prima", "kg"), new TipoRegistro("Insumos líquidos", "litro"));
        if (!await db.MotivosMerma.AnyAsync(ct))
            db.MotivosMerma.AddRange(
                new MotivoMerma("Daño en manipulación", "Deterioro durante el movimiento interno.", true, false),
                new MotivoMerma("Vencimiento", "Producto fuera de vigencia.", true, true),
                new MotivoMerma("Derrame", "Pérdida accidental de líquidos.", true, false),
                new MotivoMerma("Diferencia de inventario", "Diferencia detectada en conteo.", false, true),
                new MotivoMerma("Defecto de origen", "Material recibido con defecto.", true, false));
        if (!await db.Trabajadores.AnyAsync(ct))
        {
            for (var i = 1; i <= 10; i++)
            {
                var trabajador = new Trabajador($"99.999.{i:000}-K", $"Trabajador Demo {i:00}", i % 2 == 0 ? "Operaciones" : "Bodega", "seed");
                if (i == 10) trabajador.Desactivar("seed");
                db.Trabajadores.Add(trabajador);
            }
        }
        await db.SaveChangesAsync(ct);

        if (!await db.Entradas.AnyAsync(ct))
        {
            var tipo = await db.TiposRegistro.OrderBy(x => x.Id).FirstAsync(ct);
            var trabajadores = await db.Trabajadores.Where(x => x.Estado == SistemaGestion.Domain.Enums.EstadoCatalogo.Activo).OrderBy(x => x.Id).Take(2).ToListAsync(ct);
            if (trabajadores.Count > 0)
            {
                var segundoTrabajador = trabajadores.ElementAtOrDefault(1) ?? trabajadores[0];
                var entradaCompleta = new Entrada(tipo.Id, DateTime.UtcNow.AddDays(-2), 10m, "DEMO-ENT-001", "seed", "Entrada demostrativa sin saldo.");
                var entradaParcial = new Entrada(tipo.Id, DateTime.UtcNow.AddDays(-1), 25m, "DEMO-ENT-002", "seed", "Entrada demostrativa con saldo.");
                db.Entradas.AddRange(entradaCompleta, entradaParcial);
                await db.SaveChangesAsync(ct);
                db.Asignaciones.AddRange(
                    new Asignacion(entradaCompleta.Id, trabajadores[0].Id, DateTime.UtcNow.AddDays(-2), 10m, "seed", "Consumo completo de demostración."),
                    new Asignacion(entradaParcial.Id, segundoTrabajador.Id, DateTime.UtcNow.AddDays(-1), 7.5m, "seed", "Asignación parcial de demostración."));
                entradaCompleta.RegistrarMovimiento(DateTime.UtcNow);
                entradaParcial.RegistrarMovimiento(DateTime.UtcNow);
                await db.SaveChangesAsync(ct);
            }
        }

        if (!await db.Mermas.AnyAsync(ct))
        {
            var entrada = await db.Entradas.SingleOrDefaultAsync(x => x.DocumentoOrigen == "DEMO-ENT-002", ct);
            var motivos = await db.MotivosMerma.OrderBy(x => x.Id).Take(2).ToListAsync(ct);
            if (entrada is not null && motivos.Count >= 2)
            {
                var vigente = new Merma(entrada.Id, motivos[0].Id, DateTime.UtcNow.AddHours(-12), 1.25m, "seed",
                    motivos[0].RequiereEvidencia, "EVID-DEMO-001", "Merma vigente de demostración.");
                var anulada = new Merma(entrada.Id, motivos[1].Id, DateTime.UtcNow.AddHours(-8), 0.5m, "seed",
                    motivos[1].RequiereEvidencia, "EVID-DEMO-002", "Merma anulada de demostración.");
                anulada.Anular("seed", "Dato preparado para demostrar el historial.", DateTime.UtcNow.AddHours(-7));
                db.Mermas.AddRange(vigente, anulada);
                entrada.RegistrarMovimiento(DateTime.UtcNow);
                await db.SaveChangesAsync(ct);
            }
        }
    }
}
