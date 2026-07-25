using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaGestion.Domain.Entities;

namespace SistemaGestion.Infrastructure.Persistence.Configurations;

public sealed class EntradaConfiguration : IEntityTypeConfiguration<Entrada>
{
    public void Configure(EntityTypeBuilder<Entrada> b)
    {
        b.ToTable("Entradas", t =>
        {
            t.HasCheckConstraint("CK_Entradas_Cantidad", "[CantidadInicial] > 0");
            t.HasCheckConstraint("CK_Entradas_Estado", "[Estado] IN (1,2)");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.CantidadInicial).HasPrecision(18, 3);
        b.Property(x => x.DocumentoOrigen).HasMaxLength(100).IsRequired();
        b.Property(x => x.Observacion).HasMaxLength(1000);
        b.Property(x => x.UsuarioResponsableId).HasMaxLength(450).IsRequired();
        b.Property(x => x.MotivoAnulacion).HasMaxLength(500);
        b.Property(x => x.RowVersion).IsRowVersion();
        b.HasIndex(x => new { x.DocumentoOrigen, x.TipoRegistroId }).IsUnique();
        b.HasIndex(x => x.FechaHora); b.HasIndex(x => x.TipoRegistroId); b.HasIndex(x => x.Estado);
        b.HasOne(x => x.TipoRegistro).WithMany().HasForeignKey(x => x.TipoRegistroId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class AsignacionConfiguration : IEntityTypeConfiguration<Asignacion>
{
    public void Configure(EntityTypeBuilder<Asignacion> b)
    {
        b.ToTable("Asignaciones", t =>
        {
            t.HasCheckConstraint("CK_Asignaciones_Cantidad", "[Cantidad] > 0");
            t.HasCheckConstraint("CK_Asignaciones_Estado", "[Estado] IN (1,2)");
        });
        b.HasKey(x => x.Id); b.Property(x => x.Cantidad).HasPrecision(18, 3);
        b.Property(x => x.Observacion).HasMaxLength(1000); b.Property(x => x.MotivoAnulacion).HasMaxLength(500);
        b.Property(x => x.UsuarioResponsableId).HasMaxLength(450).IsRequired();
        b.Property(x => x.RowVersion).IsRowVersion();
        b.HasIndex(x => x.EntradaId); b.HasIndex(x => x.TrabajadorId); b.HasIndex(x => x.FechaHora); b.HasIndex(x => x.Estado);
        b.HasOne(x => x.Entrada).WithMany(x => x.Asignaciones).HasForeignKey(x => x.EntradaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Trabajador).WithMany().HasForeignKey(x => x.TrabajadorId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class MermaConfiguration : IEntityTypeConfiguration<Merma>
{
    public void Configure(EntityTypeBuilder<Merma> b)
    {
        b.ToTable("Mermas", t =>
        {
            t.HasCheckConstraint("CK_Mermas_Cantidad", "[Cantidad] > 0");
            t.HasCheckConstraint("CK_Mermas_Estado", "[Estado] IN (1,2)");
        });
        b.HasKey(x => x.Id); b.Property(x => x.Cantidad).HasPrecision(18, 3);
        b.Property(x => x.Observacion).HasMaxLength(1000); b.Property(x => x.EvidenciaReferencia).HasMaxLength(500);
        b.Property(x => x.MotivoAnulacion).HasMaxLength(500); b.Property(x => x.UsuarioResponsableId).HasMaxLength(450).IsRequired();
        b.Property(x => x.RowVersion).IsRowVersion();
        b.HasIndex(x => x.EntradaId); b.HasIndex(x => x.MotivoMermaId); b.HasIndex(x => x.FechaHora); b.HasIndex(x => x.Estado);
        b.HasOne(x => x.Entrada).WithMany(x => x.Mermas).HasForeignKey(x => x.EntradaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.MotivoMerma).WithMany().HasForeignKey(x => x.MotivoMermaId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class AuditoriaConfiguration : IEntityTypeConfiguration<Auditoria>
{
    public void Configure(EntityTypeBuilder<Auditoria> b)
    {
        b.ToTable("Auditorias"); b.HasKey(x => x.Id);
        b.Property(x => x.NombreUsuario).HasMaxLength(256).IsRequired();
        b.Property(x => x.Accion).HasMaxLength(100).IsRequired(); b.Property(x => x.Entidad).HasMaxLength(100).IsRequired();
        b.Property(x => x.ClavePrimaria).HasMaxLength(100).IsRequired(); b.Property(x => x.CorrelationId).HasMaxLength(100).IsRequired();
        b.HasIndex(x => x.FechaHora); b.HasIndex(x => new { x.Entidad, x.ClavePrimaria });
    }
}

