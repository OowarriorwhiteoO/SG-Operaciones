using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SistemaGestion.Domain.Entities;
using SistemaGestion.Infrastructure.Identity;

namespace SistemaGestion.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Trabajador> Trabajadores => Set<Trabajador>();
    public DbSet<TipoRegistro> TiposRegistro => Set<TipoRegistro>();
    public DbSet<MotivoMerma> MotivosMerma => Set<MotivoMerma>();
    public DbSet<Entrada> Entradas => Set<Entrada>();
    public DbSet<Asignacion> Asignaciones => Set<Asignacion>();
    public DbSet<Merma> Mermas => Set<Merma>();
    public DbSet<Auditoria> Auditorias => Set<Auditoria>();
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<ProductoServicio> ProductosServicios => Set<ProductoServicio>();
    public DbSet<Cotizacion> Cotizaciones => Set<Cotizacion>();
    public DbSet<CotizacionDetalle> CotizacionDetalles => Set<CotizacionDetalle>();
    public DbSet<Factura> Facturas => Set<Factura>();
    public DbSet<FacturaDetalle> FacturaDetalles => Set<FacturaDetalle>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
