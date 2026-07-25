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

        await CargarDatosOperacionalesAsync(db, ct);
    }

    private static async Task CargarDatosOperacionalesAsync(ApplicationDbContext db, CancellationToken ct)
    {
        const string prefijoDocumento = "SGO-CARGA-";

        // El documento marcador hace que la carga pueda ejecutarse varias veces sin duplicar información.
        if (await db.Entradas.AnyAsync(x => x.DocumentoOrigen.StartsWith(prefijoDocumento), ct))
            return;

        var trabajadoresCarga = new (string Rut, string Nombre, string Area)[]
        {
            ("90.100.001-1", "Camila Rojas", "Bodega"),
            ("90.100.002-2", "Matías Soto", "Operaciones"),
            ("90.100.003-3", "Valentina Pérez", "Mantenimiento"),
            ("90.100.004-4", "Diego Morales", "Logística"),
            ("90.100.005-5", "Fernanda Silva", "Producción"),
            ("90.100.006-6", "Nicolás Herrera", "Bodega"),
            ("90.100.007-7", "Javiera Castro", "Operaciones"),
            ("90.100.008-8", "Sebastián Muñoz", "Mantenimiento"),
            ("90.100.009-9", "Daniela Contreras", "Logística"),
            ("90.100.010-K", "Tomás Valdés", "Producción"),
            ("90.100.011-8", "Antonia Reyes", "Bodega"),
            ("90.100.012-6", "Benjamín Navarro", "Operaciones"),
            ("90.100.013-4", "Isidora Vega", "Mantenimiento"),
            ("90.100.014-2", "Vicente Fuentes", "Logística")
        };
        var rutsExistentes = (await db.Trabajadores.Select(x => x.Rut).ToListAsync(ct)).ToHashSet();
        foreach (var trabajador in trabajadoresCarga.Where(x => !rutsExistentes.Contains(x.Rut)))
            db.Trabajadores.Add(new Trabajador(trabajador.Rut, trabajador.Nombre, trabajador.Area, "carga-inicial"));
        await db.SaveChangesAsync(ct);

        var tipos = await db.TiposRegistro.Where(x => x.Estado == SistemaGestion.Domain.Enums.EstadoCatalogo.Activo)
            .OrderBy(x => x.Id).ToListAsync(ct);
        var motivos = await db.MotivosMerma.Where(x => x.Estado == SistemaGestion.Domain.Enums.EstadoCatalogo.Activo)
            .OrderBy(x => x.Id).ToListAsync(ct);
        var trabajadores = await db.Trabajadores.Where(x => x.Estado == SistemaGestion.Domain.Enums.EstadoCatalogo.Activo)
            .OrderBy(x => x.Id).ToListAsync(ct);
        if (tipos.Count == 0 || motivos.Count == 0 || trabajadores.Count == 0)
            return;

        var usuarioId = await db.Users.OrderBy(x => x.Id).Select(x => x.Id).FirstOrDefaultAsync(ct) ?? "sistema";
        var hoyUtc = DateTime.UtcNow.Date;
        var entradas = new List<Entrada>();

        // La serie determinista cubre distintos tipos, fechas y cantidades para alimentar todos los indicadores.
        for (var indice = 1; indice <= 48; indice++)
        {
            var tipo = tipos[(indice - 1) % tipos.Count];
            var fecha = hoyUtc.AddDays(-(indice * 5 % 120)).AddHours(7 + indice % 10);
            var cantidad = 40m + indice * 3.75m + (indice % 4) * 12m;
            var entrada = new Entrada(
                tipo.Id,
                fecha,
                cantidad,
                $"{prefijoDocumento}{indice:000}",
                usuarioId,
                $"Recepción operacional planificada #{indice:000}.");
            entradas.Add(entrada);
            db.Entradas.Add(entrada);
        }
        await db.SaveChangesAsync(ct);

        for (var indice = 1; indice <= entradas.Count; indice++)
        {
            var entrada = entradas[indice - 1];
            var trabajador = trabajadores[(indice * 3) % trabajadores.Count];
            var fechaMovimiento = entrada.FechaHora.AddHours(2);
            var asignacionPrincipal = new Asignacion(
                entrada.Id,
                trabajador.Id,
                fechaMovimiento,
                decimal.Round(entrada.CantidadInicial * 0.32m, 3),
                usuarioId,
                "Entrega programada para operación.");
            if (indice % 11 == 0)
                asignacionPrincipal.Anular(usuarioId, "Registro sustituido durante la carga inicial.", fechaMovimiento.AddHours(1));
            db.Asignaciones.Add(asignacionPrincipal);

            if (indice % 3 == 0)
            {
                var segundoTrabajador = trabajadores[(indice * 5 + 1) % trabajadores.Count];
                db.Asignaciones.Add(new Asignacion(
                    entrada.Id,
                    segundoTrabajador.Id,
                    fechaMovimiento.AddDays(1),
                    decimal.Round(entrada.CantidadInicial * 0.14m, 3),
                    usuarioId,
                    "Reposición complementaria."));
            }

            if (indice % 2 == 0)
            {
                var motivo = motivos[indice % motivos.Count];
                var merma = new Merma(
                    entrada.Id,
                    motivo.Id,
                    fechaMovimiento.AddHours(3),
                    decimal.Round(entrada.CantidadInicial * (0.015m + indice % 4 * 0.005m), 3),
                    usuarioId,
                    motivo.RequiereEvidencia,
                    motivo.RequiereEvidencia ? $"EVID-SGO-{indice:000}" : null,
                    "Hallazgo registrado durante control operacional.");
                if (indice % 14 == 0)
                    merma.Anular(usuarioId, "Medición rectificada después de la revisión.", fechaMovimiento.AddHours(4));
                db.Mermas.Add(merma);
            }

            entrada.RegistrarMovimiento(fechaMovimiento);
            db.Auditorias.Add(new Auditoria(
                usuarioId,
                "Carga inicial",
                "Crear",
                nameof(Entrada),
                entrada.Id.ToString(),
                entrada.FechaHora,
                $"carga-sgo-{indice:000}",
                valoresNuevos: $"Documento={entrada.DocumentoOrigen};Cantidad={entrada.CantidadInicial}",
                motivo: "Población inicial de datos operacionales."));
        }

        await db.SaveChangesAsync(ct);
    }
}
