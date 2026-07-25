using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaGestion.Domain.Entities;

namespace SistemaGestion.Infrastructure.Persistence.Configurations;

public sealed class TrabajadorConfiguration : IEntityTypeConfiguration<Trabajador>
{
    public void Configure(EntityTypeBuilder<Trabajador> b)
    {
        b.ToTable("Trabajadores", t => t.HasCheckConstraint("CK_Trabajadores_Estado", "[Estado] IN (1,2)"));
        b.HasKey(x => x.Id);
        b.Property(x => x.Rut).HasMaxLength(20).IsRequired();
        b.HasIndex(x => x.Rut).IsUnique();
        b.Property(x => x.NombreCompleto).HasMaxLength(150).IsRequired();
        b.Property(x => x.Area).HasMaxLength(100).IsRequired();
        b.Property(x => x.CreadoPor).HasMaxLength(450).IsRequired();
        b.Property(x => x.ModificadoPor).HasMaxLength(450);
        b.Property(x => x.RowVersion).IsRowVersion();
    }
}

public sealed class TipoRegistroConfiguration : IEntityTypeConfiguration<TipoRegistro>
{
    public void Configure(EntityTypeBuilder<TipoRegistro> b)
    {
        b.ToTable("TiposRegistro", t => t.HasCheckConstraint("CK_TiposRegistro_Estado", "[Estado] IN (1,2)"));
        b.HasKey(x => x.Id);
        b.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
        b.HasIndex(x => x.Nombre).IsUnique();
        b.Property(x => x.UnidadMedida).HasMaxLength(30).IsRequired();
        b.Property(x => x.RowVersion).IsRowVersion();
    }
}

public sealed class MotivoMermaConfiguration : IEntityTypeConfiguration<MotivoMerma>
{
    public void Configure(EntityTypeBuilder<MotivoMerma> b)
    {
        b.ToTable("MotivosMerma", t => t.HasCheckConstraint("CK_MotivosMerma_Estado", "[Estado] IN (1,2)"));
        b.HasKey(x => x.Id);
        b.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
        b.HasIndex(x => x.Nombre).IsUnique();
        b.Property(x => x.Descripcion).HasMaxLength(500);
        b.Property(x => x.RowVersion).IsRowVersion();
    }
}

