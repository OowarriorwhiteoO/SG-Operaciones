using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SistemaGestion.Application.Abstractions;
using SistemaGestion.Application.Common;
using SistemaGestion.Application.DTOs;
using SistemaGestion.Domain.Entities;
using SistemaGestion.Domain.Enums;
using SistemaGestion.Domain.Exceptions;
using SistemaGestion.Infrastructure.Persistence;

namespace SistemaGestion.Infrastructure.Services;

public sealed class MermaService(
    ApplicationDbContext db,
    ICurrentUserService current,
    IDateTimeProvider clock,
    IAuditoriaService auditoria) : IMermaService
{
    public async Task<PagedResult<MermaListItemDto>> ListarAsync(MermaFiltro filtro, CancellationToken ct)
    {
        ValidarRango(filtro.FechaDesde, filtro.FechaHasta);
        var query = db.Mermas.AsNoTracking().AsQueryable();
        if (filtro.FechaDesde.HasValue) query = query.Where(x => x.FechaHora >= filtro.FechaDesde.Value.Date);
        if (filtro.FechaHasta.HasValue) query = query.Where(x => x.FechaHora < filtro.FechaHasta.Value.Date.AddDays(1));
        if (filtro.MotivoMermaId.HasValue) query = query.Where(x => x.MotivoMermaId == filtro.MotivoMermaId);
        if (filtro.TipoRegistroId.HasValue) query = query.Where(x => x.Entrada.TipoRegistroId == filtro.TipoRegistroId);
        if (filtro.EntradaId.HasValue) query = query.Where(x => x.EntradaId == filtro.EntradaId);
        if (filtro.Estado.HasValue) query = query.Where(x => x.Estado == filtro.Estado);
        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(x => x.FechaHora).ThenByDescending(x => x.Id)
            .Skip((filtro.Pagina - 1) * filtro.TamanoPagina).Take(filtro.TamanoPagina)
            .Select(x => new MermaListItemDto(x.Id, x.FechaHora, x.EntradaId, x.Entrada.DocumentoOrigen,
                x.Entrada.TipoRegistro.Nombre, x.Entrada.TipoRegistro.UnidadMedida, x.MotivoMermaId,
                x.MotivoMerma.Nombre, x.Cantidad, x.EvidenciaReferencia, x.Estado,
                db.Users.Where(u => u.Id == x.UsuarioResponsableId).Select(u => u.Email).FirstOrDefault() ?? x.UsuarioResponsableId))
            .ToListAsync(ct);
        return new(items, filtro.Pagina, filtro.TamanoPagina, total);
    }

    public async Task<Resultado<int>> CrearAsync(MermaInput input, CancellationToken ct)
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
            var entrada = await db.Entradas.SingleOrDefaultAsync(x => x.Id == input.EntradaId, ct);
            if (entrada is null) return await FallarAsync(transaction, "La entrada no existe.", ct);
            if (entrada.Estado != EstadoMovimiento.Vigente) return await FallarAsync(transaction, "La entrada está anulada.", ct);
            if (!entrada.RowVersion.SequenceEqual(token))
                return await FallarAsync(transaction, MensajeConcurrencia, ct);
            var motivo = await db.MotivosMerma.AsNoTracking().SingleOrDefaultAsync(x => x.Id == input.MotivoMermaId, ct);
            if (motivo is null) return await FallarAsync(transaction, "El motivo de merma no existe.", ct);
            if (motivo.Estado != EstadoCatalogo.Activo) return await FallarAsync(transaction, "El motivo de merma está inactivo.", ct);
            if (motivo.RequiereEvidencia && string.IsNullOrWhiteSpace(input.EvidenciaReferencia))
                return await FallarAsync(transaction, "El motivo seleccionado requiere una referencia de evidencia.", ct);

            var asignado = await db.Asignaciones.Where(x => x.EntradaId == entrada.Id && x.Estado == EstadoMovimiento.Vigente).SumAsync(x => (decimal?)x.Cantidad, ct) ?? 0;
            var mermado = await db.Mermas.Where(x => x.EntradaId == entrada.Id && x.Estado == EstadoMovimiento.Vigente).SumAsync(x => (decimal?)x.Cantidad, ct) ?? 0;
            var disponible = entrada.CantidadInicial - asignado - mermado;
            if (input.Cantidad > disponible)
                return await FallarAsync(transaction, $"La cantidad supera el saldo disponible ({disponible:N3}).", ct);

            var entity = new Merma(entrada.Id, motivo.Id, AUtc(input.FechaHora), input.Cantidad, current.UserId,
                motivo.RequiereEvidencia, input.EvidenciaReferencia, input.Observacion);
            db.Mermas.Add(entity);
            entrada.RegistrarMovimiento(clock.UtcNow);
            await db.SaveChangesAsync(ct);
            auditoria.Registrar("Crear", nameof(Merma), entity.Id.ToString(), new
            {
                entity.EntradaId, entity.MotivoMermaId, entity.FechaHora, entity.Cantidad,
                entity.Observacion, entity.EvidenciaReferencia
            });
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return Resultado<int>.Ok(entity.Id);
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(ct);
            db.ChangeTracker.Clear();
            return Resultado<int>.Fallo(MensajeConcurrencia);
        }
        catch (Exception ex) when (EsBloqueoConcurrente(ex))
        {
            await transaction.RollbackAsync(ct);
            db.ChangeTracker.Clear();
            return Resultado<int>.Fallo(MensajeConcurrencia);
        }
        catch (DomainException ex)
        {
            await transaction.RollbackAsync(ct);
            return Resultado<int>.Fallo(ex.Message);
        }
    }

    public async Task<IndicadorMermaDto> ObtenerIndicadoresAsync(IndicadorMermaFiltro filtro, CancellationToken ct)
    {
        ValidarRango(filtro.FechaDesde, filtro.FechaHasta);
        var hastaExclusiva = filtro.FechaHasta.Date.AddDays(1);
        var entradas = db.Entradas.AsNoTracking().Where(x =>
            x.Estado == EstadoMovimiento.Vigente && x.FechaHora >= filtro.FechaDesde.Date && x.FechaHora < hastaExclusiva);
        var mermas = db.Mermas.AsNoTracking().Where(x =>
            x.Estado == EstadoMovimiento.Vigente && x.FechaHora >= filtro.FechaDesde.Date && x.FechaHora < hastaExclusiva);
        if (filtro.TipoRegistroId.HasValue)
        {
            entradas = entradas.Where(x => x.TipoRegistroId == filtro.TipoRegistroId);
            mermas = mermas.Where(x => x.Entrada.TipoRegistroId == filtro.TipoRegistroId);
        }
        var totalEntradas = await entradas.SumAsync(x => (decimal?)x.CantidadInicial, ct) ?? 0;
        var grupos = await mermas.GroupBy(x => new
            {
                x.MotivoMermaId,
                x.Entrada.TipoRegistroId,
                Motivo = x.MotivoMerma.Nombre,
                Tipo = x.Entrada.TipoRegistro.Nombre,
                Unidad = x.Entrada.TipoRegistro.UnidadMedida
            })
            .Select(g => new
            {
                g.Key.MotivoMermaId, g.Key.TipoRegistroId, g.Key.Motivo, g.Key.Tipo, g.Key.Unidad,
                Cantidad = g.Sum(x => x.Cantidad), Frecuencia = g.Count()
            })
            .OrderByDescending(x => x.Cantidad).ToListAsync(ct);
        var totalMermas = grupos.Sum(x => x.Cantidad);
        decimal acumulado = 0;
        var items = grupos.Select(x =>
        {
            var porcentajeMermas = totalMermas == 0 ? 0 : x.Cantidad / totalMermas * 100;
            acumulado += porcentajeMermas;
            return new IndicadorMermaItemDto(x.MotivoMermaId, x.TipoRegistroId, x.Motivo, x.Tipo, x.Unidad, x.Cantidad, x.Frecuencia,
                porcentajeMermas, totalEntradas == 0 ? 0 : x.Cantidad / totalEntradas * 100, acumulado);
        }).ToList();
        return new(filtro, totalMermas, totalEntradas, items);
    }

    private const string MensajeConcurrencia = "El saldo de esta entrada fue modificado por otro usuario. Actualice la información e intente nuevamente.";
    private static DateTime AUtc(DateTime value) => value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
    private static void ValidarRango(DateTime? desde, DateTime? hasta)
    {
        if (desde.HasValue && hasta.HasValue && desde.Value.Date > hasta.Value.Date)
            throw new DomainException("La fecha desde no puede ser posterior a la fecha hasta.");
    }
    private static async Task<Resultado<int>> FallarAsync(IDbContextTransaction transaction, string error, CancellationToken ct)
    {
        await transaction.RollbackAsync(ct);
        return Resultado<int>.Fallo(error);
    }
    private static bool EsBloqueoConcurrente(Exception exception)
    {
        for (var currentException = exception; currentException is not null; currentException = currentException.InnerException)
            if (currentException is SqlException { Number: 1205 or 1222 }) return true;
        return false;
    }
}

public sealed class AnulacionService(
    ApplicationDbContext db,
    ICurrentUserService current,
    IDateTimeProvider clock,
    IAuditoriaService auditoria) : IAnulacionService
{
    public Task<AnulacionDetalleDto?> ObtenerAsync(ClaseMovimiento clase, int id, CancellationToken ct) => clase switch
    {
        ClaseMovimiento.Entrada => db.Entradas.AsNoTracking().Where(x => x.Id == id)
            .Select(x => new AnulacionDetalleDto(x.Id, clase, x.DocumentoOrigen, x.FechaHora, x.CantidadInicial,
                x.TipoRegistro.UnidadMedida, x.Estado, x.RowVersion)).SingleOrDefaultAsync(ct),
        ClaseMovimiento.Asignacion => db.Asignaciones.AsNoTracking().Where(x => x.Id == id)
            .Select(x => new AnulacionDetalleDto(x.Id, clase, x.Entrada.DocumentoOrigen + " · " + x.Trabajador.NombreCompleto,
                x.FechaHora, x.Cantidad, x.Entrada.TipoRegistro.UnidadMedida, x.Estado, x.RowVersion)).SingleOrDefaultAsync(ct),
        ClaseMovimiento.Merma => db.Mermas.AsNoTracking().Where(x => x.Id == id)
            .Select(x => new AnulacionDetalleDto(x.Id, clase, x.Entrada.DocumentoOrigen + " · " + x.MotivoMerma.Nombre,
                x.FechaHora, x.Cantidad, x.Entrada.TipoRegistro.UnidadMedida, x.Estado, x.RowVersion)).SingleOrDefaultAsync(ct),
        _ => Task.FromResult<AnulacionDetalleDto?>(null)
    };

    public async Task<Resultado> AnularAsync(AnulacionInput input, CancellationToken ct)
    {
        if (current.UserId is null) return Resultado.Fallo("Debe iniciar sesión.");
        if (string.IsNullOrWhiteSpace(input.Motivo)) return Resultado.Fallo("El motivo de anulación es obligatorio.");
        byte[] token;
        try { token = Convert.FromBase64String(input.RowVersion); }
        catch (FormatException) { return Resultado.Fallo("El token de concurrencia no es válido."); }
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        try
        {
            var result = input.Clase switch
            {
                ClaseMovimiento.Entrada => await AnularEntradaAsync(input, token, ct),
                ClaseMovimiento.Asignacion => await AnularAsignacionAsync(input, token, ct),
                ClaseMovimiento.Merma => await AnularMermaAsync(input, token, ct),
                _ => Resultado.Fallo("La clase de movimiento no es válida.")
            };
            if (!result.Exitoso)
            {
                await transaction.RollbackAsync(ct);
                return result;
            }
            await db.SaveChangesAsync(ct);
            auditoria.Registrar("Anular", input.Clase.ToString(), input.Id.ToString(),
                new { Estado = EstadoMovimiento.Anulada, Motivo = input.Motivo }, motivo: input.Motivo);
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return Resultado.Ok();
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(ct);
            db.ChangeTracker.Clear();
            return Resultado.Fallo("El registro fue modificado por otro usuario. Actualice la información e intente nuevamente.");
        }
        catch (Exception ex) when (EsBloqueoConcurrente(ex))
        {
            await transaction.RollbackAsync(ct);
            db.ChangeTracker.Clear();
            return Resultado.Fallo("El registro está siendo modificado por otro usuario. Intente nuevamente.");
        }
        catch (DomainException ex)
        {
            await transaction.RollbackAsync(ct);
            return Resultado.Fallo(ex.Message);
        }
    }

    private async Task<Resultado> AnularEntradaAsync(AnulacionInput input, byte[] token, CancellationToken ct)
    {
        var entity = await db.Entradas.SingleOrDefaultAsync(x => x.Id == input.Id, ct);
        if (entity is null) return Resultado.Fallo("La entrada no existe.");
        if (!entity.RowVersion.SequenceEqual(token)) return Resultado.Fallo("La entrada fue modificada por otro usuario.");
        var tieneMovimientos = await db.Asignaciones.AnyAsync(x => x.EntradaId == entity.Id && x.Estado == EstadoMovimiento.Vigente, ct)
            || await db.Mermas.AnyAsync(x => x.EntradaId == entity.Id && x.Estado == EstadoMovimiento.Vigente, ct);
        entity.Anular(current.UserId!, input.Motivo, clock.UtcNow, tieneMovimientos);
        return Resultado.Ok();
    }

    private async Task<Resultado> AnularAsignacionAsync(AnulacionInput input, byte[] token, CancellationToken ct)
    {
        var entity = await db.Asignaciones.Include(x => x.Entrada).SingleOrDefaultAsync(x => x.Id == input.Id, ct);
        if (entity is null) return Resultado.Fallo("La asignación no existe.");
        if (!entity.RowVersion.SequenceEqual(token)) return Resultado.Fallo("La asignación fue modificada por otro usuario.");
        entity.Anular(current.UserId!, input.Motivo, clock.UtcNow);
        entity.Entrada.RegistrarMovimiento(clock.UtcNow);
        return Resultado.Ok();
    }

    private async Task<Resultado> AnularMermaAsync(AnulacionInput input, byte[] token, CancellationToken ct)
    {
        var entity = await db.Mermas.Include(x => x.Entrada).SingleOrDefaultAsync(x => x.Id == input.Id, ct);
        if (entity is null) return Resultado.Fallo("La merma no existe.");
        if (!entity.RowVersion.SequenceEqual(token)) return Resultado.Fallo("La merma fue modificada por otro usuario.");
        entity.Anular(current.UserId!, input.Motivo, clock.UtcNow);
        entity.Entrada.RegistrarMovimiento(clock.UtcNow);
        return Resultado.Ok();
    }

    private static bool EsBloqueoConcurrente(Exception exception)
    {
        for (var currentException = exception; currentException is not null; currentException = currentException.InnerException)
            if (currentException is SqlException { Number: 1205 or 1222 }) return true;
        return false;
    }
}
