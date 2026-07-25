using System.Data;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SistemaGestion.Application.Abstractions;
using SistemaGestion.Application.Common;
using SistemaGestion.Application.DTOs;
using SistemaGestion.Domain.Entities;
using SistemaGestion.Domain.Enums;
using SistemaGestion.Domain.Exceptions;
using SistemaGestion.Infrastructure.Persistence;

namespace SistemaGestion.Infrastructure.Services;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}

public sealed class AuditoriaService(ApplicationDbContext db, ICurrentUserService current, IDateTimeProvider clock) : IAuditoriaService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public void Registrar(string accion, string entidad, string clavePrimaria, object? valoresNuevos = null, object? valoresAnteriores = null, string? motivo = null)
    {
        db.Auditorias.Add(new Auditoria(
            current.UserId, current.UserName, accion, entidad, clavePrimaria, clock.UtcNow,
            current.CorrelationId,
            valoresAnteriores is null ? null : JsonSerializer.Serialize(valoresAnteriores, JsonOptions),
            valoresNuevos is null ? null : JsonSerializer.Serialize(valoresNuevos, JsonOptions),
            motivo, current.IpAddress, Truncar(current.UserAgent, 1000)));
    }

    public async Task RegistrarYGuardarAsync(string accion, string entidad, string clavePrimaria, object? detalle, CancellationToken ct)
    {
        Registrar(accion, entidad, clavePrimaria, detalle);
        await db.SaveChangesAsync(ct);
    }

    private static string? Truncar(string? value, int max) => value?.Length > max ? value[..max] : value;
}

public sealed class SaldoService(ApplicationDbContext db) : ISaldoService
{
    public Task<SaldoDto?> ObtenerAsync(int entradaId, CancellationToken ct) =>
        db.Entradas.AsNoTracking().Where(x => x.Id == entradaId)
            .Select(x => new SaldoDto(
                x.Id,
                x.CantidadInicial,
                x.Asignaciones.Where(a => a.Estado == EstadoMovimiento.Vigente).Sum(a => (decimal?)a.Cantidad) ?? 0,
                x.Mermas.Where(m => m.Estado == EstadoMovimiento.Vigente).Sum(m => (decimal?)m.Cantidad) ?? 0,
                x.CantidadInicial
                    - (x.Asignaciones.Where(a => a.Estado == EstadoMovimiento.Vigente).Sum(a => (decimal?)a.Cantidad) ?? 0)
                    - (x.Mermas.Where(m => m.Estado == EstadoMovimiento.Vigente).Sum(m => (decimal?)m.Cantidad) ?? 0),
                x.RowVersion))
            .SingleOrDefaultAsync(ct);
}

public sealed class EntradaService(
    ApplicationDbContext db,
    ICurrentUserService current,
    IAuditoriaService auditoria) : IEntradaService
{
    public async Task<PagedResult<EntradaListItemDto>> ListarAsync(EntradaFiltro filtro, CancellationToken ct)
    {
        ValidarRango(filtro.FechaDesde, filtro.FechaHasta);
        var query = db.Entradas.AsNoTracking().AsQueryable();
        if (filtro.FechaDesde.HasValue) query = query.Where(x => x.FechaHora >= filtro.FechaDesde.Value.Date);
        if (filtro.FechaHasta.HasValue) query = query.Where(x => x.FechaHora < filtro.FechaHasta.Value.Date.AddDays(1));
        if (filtro.TipoRegistroId.HasValue) query = query.Where(x => x.TipoRegistroId == filtro.TipoRegistroId);
        if (!string.IsNullOrWhiteSpace(filtro.DocumentoOrigen))
        {
            var documento = Entrada.NormalizarDocumento(filtro.DocumentoOrigen);
            query = query.Where(x => x.DocumentoOrigen.Contains(documento));
        }
        if (filtro.Estado.HasValue) query = query.Where(x => x.Estado == filtro.Estado);
        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(x => x.FechaHora).ThenByDescending(x => x.Id)
            .Skip((filtro.Pagina - 1) * filtro.TamanoPagina).Take(filtro.TamanoPagina)
            .Select(x => new EntradaListItemDto(
                x.Id, x.FechaHora, x.TipoRegistro.Nombre, x.TipoRegistro.UnidadMedida, x.CantidadInicial,
                x.Asignaciones.Where(a => a.Estado == EstadoMovimiento.Vigente).Sum(a => (decimal?)a.Cantidad) ?? 0,
                x.Mermas.Where(m => m.Estado == EstadoMovimiento.Vigente).Sum(m => (decimal?)m.Cantidad) ?? 0,
                x.CantidadInicial
                    - (x.Asignaciones.Where(a => a.Estado == EstadoMovimiento.Vigente).Sum(a => (decimal?)a.Cantidad) ?? 0)
                    - (x.Mermas.Where(m => m.Estado == EstadoMovimiento.Vigente).Sum(m => (decimal?)m.Cantidad) ?? 0),
                x.DocumentoOrigen, x.Estado,
                db.Users.Where(u => u.Id == x.UsuarioResponsableId).Select(u => u.Email).FirstOrDefault() ?? x.UsuarioResponsableId,
                x.RowVersion))
            .ToListAsync(ct);
        return new(items, filtro.Pagina, filtro.TamanoPagina, total);
    }

    public async Task<EntradaDetalleDto?> ObtenerDetalleAsync(int id, CancellationToken ct)
    {
        var entry = await db.Entradas.AsNoTracking().Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id, x.FechaHora, Tipo = x.TipoRegistro.Nombre, Unidad = x.TipoRegistro.UnidadMedida,
                x.CantidadInicial, x.DocumentoOrigen, x.Observacion, x.Estado,
                UsuarioResponsable = db.Users.Where(u => u.Id == x.UsuarioResponsableId).Select(u => u.Email).FirstOrDefault() ?? x.UsuarioResponsableId,
                x.RowVersion,
                TotalAsignado = x.Asignaciones.Where(a => a.Estado == EstadoMovimiento.Vigente).Sum(a => (decimal?)a.Cantidad) ?? 0,
                TotalMerma = x.Mermas.Where(m => m.Estado == EstadoMovimiento.Vigente).Sum(m => (decimal?)m.Cantidad) ?? 0
            }).SingleOrDefaultAsync(ct);
        if (entry is null) return null;
        var asignaciones = await db.Asignaciones.AsNoTracking().Where(x => x.EntradaId == id)
            .OrderByDescending(x => x.FechaHora)
            .Select(x => new AsignacionListItemDto(x.Id, x.FechaHora, x.EntradaId, x.Entrada.DocumentoOrigen,
                x.Entrada.TipoRegistro.Nombre, x.Entrada.TipoRegistro.UnidadMedida, x.TrabajadorId,
                x.Trabajador.NombreCompleto, x.Cantidad, x.Estado,
                db.Users.Where(u => u.Id == x.UsuarioResponsableId).Select(u => u.Email).FirstOrDefault() ?? x.UsuarioResponsableId)).ToListAsync(ct);
        var mermas = await db.Mermas.AsNoTracking().Where(x => x.EntradaId == id)
            .OrderByDescending(x => x.FechaHora)
            .Select(x => new MermaListItemDto(x.Id, x.FechaHora, x.EntradaId, x.Entrada.DocumentoOrigen,
                x.Entrada.TipoRegistro.Nombre, x.Entrada.TipoRegistro.UnidadMedida, x.MotivoMermaId,
                x.MotivoMerma.Nombre, x.Cantidad, x.EvidenciaReferencia, x.Estado,
                db.Users.Where(u => u.Id == x.UsuarioResponsableId).Select(u => u.Email).FirstOrDefault() ?? x.UsuarioResponsableId))
            .ToListAsync(ct);
        return new(entry.Id, entry.FechaHora, entry.Tipo, entry.Unidad, entry.CantidadInicial,
            entry.TotalAsignado, entry.TotalMerma, entry.CantidadInicial - entry.TotalAsignado - entry.TotalMerma,
            entry.DocumentoOrigen, entry.Observacion, entry.Estado, entry.UsuarioResponsable, entry.RowVersion, asignaciones, mermas);
    }

    public async Task<IReadOnlyList<EntradaOpcionDto>> ListarDisponiblesAsync(CancellationToken ct)
    {
        var query = db.Entradas.AsNoTracking().Where(x => x.Estado == EstadoMovimiento.Vigente)
            .Select(x => new
            {
                x.Id,
                x.DocumentoOrigen,
                Tipo = x.TipoRegistro.Nombre,
                Unidad = x.TipoRegistro.UnidadMedida,
                Saldo = x.CantidadInicial
                    - (x.Asignaciones.Where(a => a.Estado == EstadoMovimiento.Vigente).Sum(a => (decimal?)a.Cantidad) ?? 0)
                    - (x.Mermas.Where(m => m.Estado == EstadoMovimiento.Vigente).Sum(m => (decimal?)m.Cantidad) ?? 0),
                x.RowVersion
            });
        return await query.Where(x => x.Saldo > 0).OrderBy(x => x.DocumentoOrigen)
            .Select(x => new EntradaOpcionDto(x.Id, x.DocumentoOrigen + " · " + x.Tipo, x.Tipo, x.Unidad, x.Saldo, x.RowVersion))
            .ToListAsync(ct);
    }

    public async Task<Resultado<int>> CrearAsync(EntradaInput input, CancellationToken ct)
    {
        if (current.UserId is null) return Resultado<int>.Fallo("Debe iniciar sesión.");
        if (input.CantidadInicial <= 0) return Resultado<int>.Fallo("La cantidad inicial debe ser mayor que cero.");
        if (string.IsNullOrWhiteSpace(input.DocumentoOrigen)) return Resultado<int>.Fallo("El documento de origen es obligatorio.");
        if (input.FechaHora == default) return Resultado<int>.Fallo("La fecha y hora son obligatorias.");
        var tipo = await db.TiposRegistro.AsNoTracking().SingleOrDefaultAsync(x => x.Id == input.TipoRegistroId, ct);
        if (tipo is null) return Resultado<int>.Fallo("El tipo de registro no existe.");
        if (tipo.Estado != EstadoCatalogo.Activo) return Resultado<int>.Fallo("El tipo de registro está inactivo.");
        var documento = Entrada.NormalizarDocumento(input.DocumentoOrigen);
        if (await db.Entradas.AnyAsync(x => x.TipoRegistroId == input.TipoRegistroId && x.DocumentoOrigen == documento, ct))
            return Resultado<int>.Fallo("Ya existe una entrada con el mismo documento y tipo.");

        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        try
        {
            var entity = new Entrada(input.TipoRegistroId, AUtc(input.FechaHora), input.CantidadInicial, documento, current.UserId, input.Observacion);
            db.Entradas.Add(entity);
            await db.SaveChangesAsync(ct);
            auditoria.Registrar("Crear", nameof(Entrada), entity.Id.ToString(), new
            {
                entity.TipoRegistroId, entity.FechaHora, entity.CantidadInicial, entity.DocumentoOrigen, entity.Observacion
            });
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return Resultado<int>.Ok(entity.Id);
        }
        catch (DbUpdateException ex) when (EsDuplicado(ex))
        {
            await transaction.RollbackAsync(ct);
            return Resultado<int>.Fallo("Ya existe una entrada con el mismo documento y tipo.");
        }
        catch (DomainException ex)
        {
            await transaction.RollbackAsync(ct);
            return Resultado<int>.Fallo(ex.Message);
        }
    }

    private static bool EsDuplicado(DbUpdateException ex) => ex.InnerException is SqlException { Number: 2601 or 2627 };
    private static DateTime AUtc(DateTime value) => value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
    private static void ValidarRango(DateTime? desde, DateTime? hasta)
    {
        if (desde.HasValue && hasta.HasValue && desde.Value.Date > hasta.Value.Date)
            throw new DomainException("La fecha desde no puede ser posterior a la fecha hasta.");
    }
}

public sealed class AsignacionService(
    ApplicationDbContext db,
    ICurrentUserService current,
    IDateTimeProvider clock,
    IAuditoriaService auditoria) : IAsignacionService
{
    public async Task<PagedResult<AsignacionListItemDto>> ListarAsync(AsignacionFiltro filtro, CancellationToken ct)
    {
        if (filtro.FechaDesde.HasValue && filtro.FechaHasta.HasValue && filtro.FechaDesde.Value.Date > filtro.FechaHasta.Value.Date)
            throw new DomainException("La fecha desde no puede ser posterior a la fecha hasta.");
        var query = db.Asignaciones.AsNoTracking().AsQueryable();
        if (filtro.FechaDesde.HasValue) query = query.Where(x => x.FechaHora >= filtro.FechaDesde.Value.Date);
        if (filtro.FechaHasta.HasValue) query = query.Where(x => x.FechaHora < filtro.FechaHasta.Value.Date.AddDays(1));
        if (filtro.TrabajadorId.HasValue) query = query.Where(x => x.TrabajadorId == filtro.TrabajadorId);
        if (filtro.EntradaId.HasValue) query = query.Where(x => x.EntradaId == filtro.EntradaId);
        if (filtro.Estado.HasValue) query = query.Where(x => x.Estado == filtro.Estado);
        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(x => x.FechaHora).ThenByDescending(x => x.Id)
            .Skip((filtro.Pagina - 1) * filtro.TamanoPagina).Take(filtro.TamanoPagina)
            .Select(x => new AsignacionListItemDto(x.Id, x.FechaHora, x.EntradaId, x.Entrada.DocumentoOrigen,
                x.Entrada.TipoRegistro.Nombre, x.Entrada.TipoRegistro.UnidadMedida, x.TrabajadorId,
                x.Trabajador.NombreCompleto, x.Cantidad, x.Estado,
                db.Users.Where(u => u.Id == x.UsuarioResponsableId).Select(u => u.Email).FirstOrDefault() ?? x.UsuarioResponsableId)).ToListAsync(ct);
        return new(items, filtro.Pagina, filtro.TamanoPagina, total);
    }

    public async Task<Resultado<int>> CrearAsync(AsignacionInput input, CancellationToken ct)
    {
        if (current.UserId is null) return Resultado<int>.Fallo("Debe iniciar sesión.");
        if (input.Cantidad <= 0) return Resultado<int>.Fallo("La cantidad debe ser mayor que cero.");
        if (input.FechaHora == default) return Resultado<int>.Fallo("La fecha y hora son obligatorias.");
        byte[] token;
        try { token = Convert.FromBase64String(input.EntradaRowVersion); }
        catch (FormatException) { return Resultado<int>.Fallo("El token de concurrencia no es válido."); }

        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        try
        {
            // El bloqueo de actualización serializa movimientos sobre la misma entrada y evita
            // que dos solicitudes con el mismo token queden esperando al intentar convertir bloqueos.
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"SET LOCK_TIMEOUT 10000; SELECT [Id] FROM [Entradas] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = {input.EntradaId}",
                ct);
            var entrada = await db.Entradas.SingleOrDefaultAsync(x => x.Id == input.EntradaId, ct);
            if (entrada is null) return await FallarAsync(transaction, "La entrada no existe.", ct);
            if (entrada.Estado != EstadoMovimiento.Vigente) return await FallarAsync(transaction, "La entrada está anulada.", ct);
            if (!entrada.RowVersion.SequenceEqual(token))
                return await FallarAsync(transaction, "El saldo de esta entrada fue modificado por otro usuario. Actualice la información e intente nuevamente.", ct);
            var trabajador = await db.Trabajadores.AsNoTracking().SingleOrDefaultAsync(x => x.Id == input.TrabajadorId, ct);
            if (trabajador is null) return await FallarAsync(transaction, "El trabajador no existe.", ct);
            if (trabajador.Estado != EstadoCatalogo.Activo) return await FallarAsync(transaction, "El trabajador está inactivo.", ct);

            var asignado = await db.Asignaciones.Where(x => x.EntradaId == entrada.Id && x.Estado == EstadoMovimiento.Vigente).SumAsync(x => (decimal?)x.Cantidad, ct) ?? 0;
            var mermado = await db.Mermas.Where(x => x.EntradaId == entrada.Id && x.Estado == EstadoMovimiento.Vigente).SumAsync(x => (decimal?)x.Cantidad, ct) ?? 0;
            var disponible = entrada.CantidadInicial - asignado - mermado;
            if (input.Cantidad > disponible)
                return await FallarAsync(transaction, $"La cantidad supera el saldo disponible ({disponible:N3}).", ct);

            var entity = new Asignacion(entrada.Id, trabajador.Id, AUtc(input.FechaHora), input.Cantidad, current.UserId, input.Observacion);
            db.Asignaciones.Add(entity);
            entrada.RegistrarMovimiento(clock.UtcNow);
            await db.SaveChangesAsync(ct);
            auditoria.Registrar("Crear", nameof(Asignacion), entity.Id.ToString(), new
            {
                entity.EntradaId, entity.TrabajadorId, entity.FechaHora, entity.Cantidad, entity.Observacion
            });
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return Resultado<int>.Ok(entity.Id);
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(ct);
            db.ChangeTracker.Clear();
            return Resultado<int>.Fallo("El saldo de esta entrada fue modificado por otro usuario. Actualice la información e intente nuevamente.");
        }
        catch (Exception ex) when (EsBloqueoConcurrente(ex))
        {
            await transaction.RollbackAsync(ct);
            db.ChangeTracker.Clear();
            return Resultado<int>.Fallo("La entrada está siendo modificada por otro usuario. Actualice la información e intente nuevamente.");
        }
        catch (DomainException ex)
        {
            await transaction.RollbackAsync(ct);
            return Resultado<int>.Fallo(ex.Message);
        }
    }

    private static async Task<Resultado<int>> FallarAsync(Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction, string error, CancellationToken ct)
    {
        await transaction.RollbackAsync(ct);
        return Resultado<int>.Fallo(error);
    }
    private static DateTime AUtc(DateTime value) => value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
    private static bool EsBloqueoConcurrente(Exception exception)
    {
        for (var currentException = exception; currentException is not null; currentException = currentException.InnerException)
            if (currentException is SqlException { Number: 1205 or 1222 }) return true;
        return false;
    }
}
