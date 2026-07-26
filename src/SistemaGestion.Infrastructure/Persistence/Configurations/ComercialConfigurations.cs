using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaGestion.Domain.Entities;

namespace SistemaGestion.Infrastructure.Persistence.Configurations;

public sealed class EmpresaConfiguration : IEntityTypeConfiguration<Empresa>
{
    public void Configure(EntityTypeBuilder<Empresa> b)
    {
        b.ToTable("Empresas"); b.HasKey(x => x.Id);
        b.Property(x => x.RazonSocial).HasMaxLength(180).IsRequired(); b.Property(x => x.NombreFantasia).HasMaxLength(150).IsRequired();
        b.Property(x => x.Rut).HasMaxLength(20).IsRequired(); b.HasIndex(x => x.Rut).IsUnique();
        b.Property(x => x.Giro).HasMaxLength(180); b.Property(x => x.Direccion).HasMaxLength(250); b.Property(x => x.Comuna).HasMaxLength(100);
        b.Property(x => x.Ciudad).HasMaxLength(100); b.Property(x => x.Email).HasMaxLength(150); b.Property(x => x.Telefono).HasMaxLength(40);
        b.Property(x => x.SitioWeb).HasMaxLength(180); b.Property(x => x.Moneda).HasMaxLength(3); b.Property(x => x.IvaPorcentaje).HasPrecision(5, 2);
        b.Property(x => x.RowVersion).IsRowVersion();
    }
}

public sealed class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> b)
    {
        b.ToTable("Clientes"); b.HasKey(x => x.Id); b.Property(x => x.Rut).HasMaxLength(20).IsRequired(); b.HasIndex(x => x.Rut).IsUnique();
        b.Property(x => x.RazonSocial).HasMaxLength(180).IsRequired(); b.Property(x => x.NombreContacto).HasMaxLength(150);
        b.Property(x => x.Email).HasMaxLength(150); b.Property(x => x.Telefono).HasMaxLength(40); b.Property(x => x.Direccion).HasMaxLength(250);
        b.Property(x => x.Comuna).HasMaxLength(100); b.Property(x => x.Ciudad).HasMaxLength(100); b.Property(x => x.RowVersion).IsRowVersion();
    }
}

public sealed class ProductoServicioConfiguration : IEntityTypeConfiguration<ProductoServicio>
{
    public void Configure(EntityTypeBuilder<ProductoServicio> b)
    {
        b.ToTable("ProductosServicios"); b.HasKey(x => x.Id); b.Property(x => x.Codigo).HasMaxLength(40).IsRequired(); b.HasIndex(x => x.Codigo).IsUnique();
        b.Property(x => x.Nombre).HasMaxLength(150).IsRequired(); b.Property(x => x.Descripcion).HasMaxLength(500);
        b.Property(x => x.UnidadMedida).HasMaxLength(30).IsRequired(); b.Property(x => x.PrecioNeto).HasPrecision(18, 2); b.Property(x => x.RowVersion).IsRowVersion();
    }
}

public sealed class CotizacionConfiguration : IEntityTypeConfiguration<Cotizacion>
{
    public void Configure(EntityTypeBuilder<Cotizacion> b)
    {
        b.ToTable("Cotizaciones"); b.HasKey(x => x.Id); b.Property(x => x.Numero).HasMaxLength(30).IsRequired(); b.HasIndex(x => x.Numero).IsUnique();
        b.Property(x => x.Observacion).HasMaxLength(1000); b.Property(x => x.UsuarioResponsableId).HasMaxLength(450).IsRequired();
        b.Property(x => x.SubtotalNeto).HasPrecision(18, 2); b.Property(x => x.MontoIva).HasPrecision(18, 2); b.Property(x => x.Total).HasPrecision(18, 2);
        b.Property(x => x.RowVersion).IsRowVersion(); b.HasOne(x => x.Cliente).WithMany().HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.Restrict);
        b.HasMany(x => x.Detalles).WithOne(x => x.Cotizacion).HasForeignKey(x => x.CotizacionId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class CotizacionDetalleConfiguration : IEntityTypeConfiguration<CotizacionDetalle>
{
    public void Configure(EntityTypeBuilder<CotizacionDetalle> b)
    {
        b.ToTable("CotizacionDetalles"); b.HasKey(x => x.Id); b.Property(x => x.Descripcion).HasMaxLength(250).IsRequired();
        b.Property(x => x.Cantidad).HasPrecision(18, 3); b.Property(x => x.PrecioUnitario).HasPrecision(18, 2);
        b.Property(x => x.DescuentoPorcentaje).HasPrecision(5, 2); b.Property(x => x.TotalNeto).HasPrecision(18, 2);
        b.Property(x => x.MontoIva).HasPrecision(18, 2); b.Property(x => x.Total).HasPrecision(18, 2);
        b.HasOne(x => x.ProductoServicio).WithMany().HasForeignKey(x => x.ProductoServicioId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class FacturaConfiguration : IEntityTypeConfiguration<Factura>
{
    public void Configure(EntityTypeBuilder<Factura> b)
    {
        b.ToTable("Facturas"); b.HasKey(x => x.Id); b.Property(x => x.Numero).HasMaxLength(30).IsRequired(); b.HasIndex(x => x.Numero).IsUnique();
        b.HasIndex(x => x.CotizacionId).IsUnique(); b.Property(x => x.SubtotalNeto).HasPrecision(18, 2); b.Property(x => x.MontoIva).HasPrecision(18, 2);
        b.Property(x => x.Total).HasPrecision(18, 2); b.Property(x => x.ReferenciaPago).HasMaxLength(100);
        b.Property(x => x.UsuarioResponsableId).HasMaxLength(450).IsRequired(); b.Property(x => x.RowVersion).IsRowVersion();
        b.HasOne(x => x.Cotizacion).WithOne().HasForeignKey<Factura>(x => x.CotizacionId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Cliente).WithMany().HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.Restrict);
        b.HasMany(x => x.Detalles).WithOne(x => x.Factura).HasForeignKey(x => x.FacturaId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class FacturaDetalleConfiguration : IEntityTypeConfiguration<FacturaDetalle>
{
    public void Configure(EntityTypeBuilder<FacturaDetalle> b)
    {
        b.ToTable("FacturaDetalles"); b.HasKey(x => x.Id); b.Property(x => x.Codigo).HasMaxLength(40).IsRequired();
        b.Property(x => x.Descripcion).HasMaxLength(250).IsRequired(); b.Property(x => x.UnidadMedida).HasMaxLength(30).IsRequired();
        b.Property(x => x.Cantidad).HasPrecision(18, 3); b.Property(x => x.PrecioUnitario).HasPrecision(18, 2);
        b.Property(x => x.DescuentoPorcentaje).HasPrecision(5, 2); b.Property(x => x.TotalNeto).HasPrecision(18, 2);
        b.Property(x => x.MontoIva).HasPrecision(18, 2); b.Property(x => x.Total).HasPrecision(18, 2);
    }
}
