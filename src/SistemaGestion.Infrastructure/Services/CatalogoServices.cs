using Microsoft.EntityFrameworkCore;
using SistemaGestion.Application.Abstractions;
using SistemaGestion.Application.Common;
using SistemaGestion.Application.DTOs;
using SistemaGestion.Domain.Entities;
using SistemaGestion.Domain.Exceptions;
using SistemaGestion.Infrastructure.Persistence;

namespace SistemaGestion.Infrastructure.Services;

public sealed class TrabajadorService(ApplicationDbContext db, ICurrentUserService current) : ITrabajadorService
{
    public async Task<IReadOnlyList<TrabajadorDto>> ListarAsync(CancellationToken ct) =>
        await db.Trabajadores.AsNoTracking().OrderBy(x => x.NombreCompleto)
            .Select(x => new TrabajadorDto(x.Id, x.Rut, x.NombreCompleto, x.Area, x.Estado, x.RowVersion)).ToListAsync(ct);
    public Task<TrabajadorDto?> ObtenerAsync(int id, CancellationToken ct) =>
        db.Trabajadores.AsNoTracking().Where(x => x.Id == id)
            .Select(x => new TrabajadorDto(x.Id, x.Rut, x.NombreCompleto, x.Area, x.Estado, x.RowVersion)).SingleOrDefaultAsync(ct);
    public async Task<Resultado> GuardarAsync(TrabajadorInput input, CancellationToken ct)
    {
        try
        {
            var rut = input.Rut.Trim().ToUpperInvariant();
            if (await db.Trabajadores.AnyAsync(x => x.Rut == rut && x.Id != input.Id, ct)) return Resultado.Fallo("Ya existe un trabajador con ese RUT.");
            if (input.Id == 0) db.Trabajadores.Add(new Trabajador(rut, input.NombreCompleto, input.Area, current.UserName));
            else
            {
                var entity = await db.Trabajadores.FindAsync([input.Id], ct);
                if (entity is null) return Resultado.Fallo("El trabajador no existe.");
                entity.Editar(input.NombreCompleto, input.Area, current.UserName);
            }
            await db.SaveChangesAsync(ct); return Resultado.Ok();
        }
        catch (DomainException ex) { return Resultado.Fallo(ex.Message); }
    }
    public async Task<Resultado> CambiarEstadoAsync(int id, bool activar, CancellationToken ct)
    {
        var entity = await db.Trabajadores.FindAsync([id], ct);
        if (entity is null) return Resultado.Fallo("El trabajador no existe.");
        if (activar) entity.Activar(current.UserName); else entity.Desactivar(current.UserName);
        await db.SaveChangesAsync(ct); return Resultado.Ok();
    }
}

public sealed class TipoRegistroService(ApplicationDbContext db) : ITipoRegistroService
{
    public async Task<IReadOnlyList<TipoRegistroDto>> ListarAsync(CancellationToken ct) =>
        await db.TiposRegistro.AsNoTracking().OrderBy(x => x.Nombre).Select(x => new TipoRegistroDto(x.Id, x.Nombre, x.UnidadMedida, x.Estado, x.RowVersion)).ToListAsync(ct);
    public Task<TipoRegistroDto?> ObtenerAsync(int id, CancellationToken ct) =>
        db.TiposRegistro.AsNoTracking().Where(x => x.Id == id).Select(x => new TipoRegistroDto(x.Id, x.Nombre, x.UnidadMedida, x.Estado, x.RowVersion)).SingleOrDefaultAsync(ct);
    public async Task<Resultado> GuardarAsync(TipoRegistroInput input, CancellationToken ct)
    {
        if (await db.TiposRegistro.AnyAsync(x => x.Nombre == input.Nombre.Trim() && x.Id != input.Id, ct)) return Resultado.Fallo("Ya existe un tipo con ese nombre.");
        if (input.Id == 0) db.TiposRegistro.Add(new TipoRegistro(input.Nombre, input.UnidadMedida));
        else { var e = await db.TiposRegistro.FindAsync([input.Id], ct); if (e is null) return Resultado.Fallo("El tipo no existe."); e.Editar(input.Nombre, input.UnidadMedida); }
        await db.SaveChangesAsync(ct); return Resultado.Ok();
    }
    public async Task<Resultado> CambiarEstadoAsync(int id, bool activar, CancellationToken ct)
    {
        var e = await db.TiposRegistro.FindAsync([id], ct); if (e is null) return Resultado.Fallo("El tipo no existe.");
        if (activar) e.Activar(); else e.Desactivar(); await db.SaveChangesAsync(ct); return Resultado.Ok();
    }
}

public sealed class MotivoMermaService(ApplicationDbContext db) : IMotivoMermaService
{
    public async Task<IReadOnlyList<MotivoMermaDto>> ListarAsync(CancellationToken ct) =>
        await db.MotivosMerma.AsNoTracking().OrderBy(x => x.Nombre).Select(x => new MotivoMermaDto(x.Id, x.Nombre, x.Descripcion, x.RequiereEvidencia, x.RequiereAutorizacion, x.Estado, x.RowVersion)).ToListAsync(ct);
    public Task<MotivoMermaDto?> ObtenerAsync(int id, CancellationToken ct) =>
        db.MotivosMerma.AsNoTracking().Where(x => x.Id == id).Select(x => new MotivoMermaDto(x.Id, x.Nombre, x.Descripcion, x.RequiereEvidencia, x.RequiereAutorizacion, x.Estado, x.RowVersion)).SingleOrDefaultAsync(ct);
    public async Task<Resultado> GuardarAsync(MotivoMermaInput input, CancellationToken ct)
    {
        if (await db.MotivosMerma.AnyAsync(x => x.Nombre == input.Nombre.Trim() && x.Id != input.Id, ct)) return Resultado.Fallo("Ya existe un motivo con ese nombre.");
        if (input.Id == 0) db.MotivosMerma.Add(new MotivoMerma(input.Nombre, input.Descripcion, input.RequiereEvidencia, input.RequiereAutorizacion));
        else { var e = await db.MotivosMerma.FindAsync([input.Id], ct); if (e is null) return Resultado.Fallo("El motivo no existe."); e.Editar(input.Nombre, input.Descripcion, input.RequiereEvidencia, input.RequiereAutorizacion); }
        await db.SaveChangesAsync(ct); return Resultado.Ok();
    }
    public async Task<Resultado> CambiarEstadoAsync(int id, bool activar, CancellationToken ct)
    {
        var e = await db.MotivosMerma.FindAsync([id], ct); if (e is null) return Resultado.Fallo("El motivo no existe.");
        if (activar) e.Activar(); else e.Desactivar(); await db.SaveChangesAsync(ct); return Resultado.Ok();
    }
}

