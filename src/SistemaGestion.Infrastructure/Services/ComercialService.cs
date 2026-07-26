using Microsoft.EntityFrameworkCore;
using SistemaGestion.Application.Abstractions;
using SistemaGestion.Application.Common;
using SistemaGestion.Application.DTOs;
using SistemaGestion.Domain.Entities;
using SistemaGestion.Domain.Enums;
using SistemaGestion.Domain.Exceptions;
using SistemaGestion.Infrastructure.Persistence;

namespace SistemaGestion.Infrastructure.Services;

public sealed class ComercialService(
    ApplicationDbContext db,
    ICurrentUserService current,
    IDateTimeProvider clock,
    IAuditoriaService auditoria) : IComercialService
{
    public async Task<EmpresaInput> ObtenerEmpresaAsync(CancellationToken ct)
    {
        var x = await db.Empresas.AsNoTracking().OrderBy(e => e.Id).FirstOrDefaultAsync(ct);
        return x is null ? new EmpresaInput { RazonSocial = "SG-Operaciones", NombreFantasia = "SG-Operaciones" } :
            new EmpresaInput { Id = x.Id, RazonSocial = x.RazonSocial, NombreFantasia = x.NombreFantasia, Rut = x.Rut, Giro = x.Giro,
                Direccion = x.Direccion, Comuna = x.Comuna, Ciudad = x.Ciudad, Email = x.Email, Telefono = x.Telefono,
                SitioWeb = x.SitioWeb, IvaPorcentaje = x.IvaPorcentaje };
    }

    public async Task<Resultado> GuardarEmpresaAsync(EmpresaInput input, CancellationToken ct)
    {
        try
        {
            var entity = await db.Empresas.OrderBy(x => x.Id).FirstOrDefaultAsync(ct);
            if (entity is null) { entity = new Empresa(input.RazonSocial, input.NombreFantasia, input.Rut); db.Empresas.Add(entity); }
            entity.Editar(input.RazonSocial, input.NombreFantasia, input.Rut, input.Giro, input.Direccion, input.Comuna,
                input.Ciudad, input.Email, input.Telefono, input.SitioWeb, input.IvaPorcentaje);
            auditoria.Registrar("Actualizar", nameof(Empresa), entity.Id.ToString(), new { input.RazonSocial, input.NombreFantasia, input.Rut });
            await db.SaveChangesAsync(ct); return Resultado.Ok();
        }
        catch (DomainException ex) { return Resultado.Fallo(ex.Message); }
    }

    public async Task<IReadOnlyList<ClienteDto>> ListarClientesAsync(CancellationToken ct) =>
        await db.Clientes.AsNoTracking().OrderBy(x => x.RazonSocial)
            .Select(x => new ClienteDto(x.Id, x.Rut, x.RazonSocial, x.NombreContacto, x.Email, x.Telefono, x.Direccion, x.Comuna, x.Ciudad, x.Estado)).ToListAsync(ct);

    public async Task<ClienteInput?> ObtenerClienteAsync(int id, CancellationToken ct) =>
        await db.Clientes.AsNoTracking().Where(x => x.Id == id).Select(x => new ClienteInput { Id = x.Id, Rut = x.Rut, RazonSocial = x.RazonSocial,
            NombreContacto = x.NombreContacto, Email = x.Email, Telefono = x.Telefono, Direccion = x.Direccion, Comuna = x.Comuna, Ciudad = x.Ciudad }).SingleOrDefaultAsync(ct);

    public async Task<Resultado> GuardarClienteAsync(ClienteInput input, CancellationToken ct)
    {
        try
        {
            var rut = input.Rut.Trim().ToUpperInvariant();
            if (await db.Clientes.AnyAsync(x => x.Rut == rut && x.Id != input.Id, ct)) return Resultado.Fallo("Ya existe un cliente con ese RUT.");
            Cliente entity;
            if (input.Id == 0) { entity = new Cliente(rut, input.RazonSocial); db.Clientes.Add(entity); }
            else { entity = await db.Clientes.FindAsync([input.Id], ct) ?? throw new DomainException("El cliente no existe."); }
            entity.Editar(rut, input.RazonSocial, input.NombreContacto, input.Email, input.Telefono, input.Direccion, input.Comuna, input.Ciudad);
            await db.SaveChangesAsync(ct); auditoria.Registrar("Guardar", nameof(Cliente), entity.Id.ToString(), new { entity.Rut, entity.RazonSocial }); await db.SaveChangesAsync(ct);
            return Resultado.Ok();
        }
        catch (DomainException ex) { return Resultado.Fallo(ex.Message); }
    }
    public async Task<Resultado> CambiarEstadoClienteAsync(int id, bool activar, CancellationToken ct)
    {
        var x = await db.Clientes.FindAsync([id], ct); if (x is null) return Resultado.Fallo("El cliente no existe.");
        x.CambiarEstado(activar); await db.SaveChangesAsync(ct); return Resultado.Ok();
    }

    public async Task<IReadOnlyList<ProductoServicioDto>> ListarProductosAsync(CancellationToken ct) =>
        await db.ProductosServicios.AsNoTracking().OrderBy(x => x.Nombre)
            .Select(x => new ProductoServicioDto(x.Id, x.Codigo, x.Nombre, x.Descripcion, x.UnidadMedida, x.PrecioNeto, x.AfectoIva, x.EsServicio, x.Estado)).ToListAsync(ct);
    public async Task<ProductoServicioInput?> ObtenerProductoAsync(int id, CancellationToken ct) =>
        await db.ProductosServicios.AsNoTracking().Where(x => x.Id == id).Select(x => new ProductoServicioInput { Id = x.Id, Codigo = x.Codigo,
            Nombre = x.Nombre, Descripcion = x.Descripcion, UnidadMedida = x.UnidadMedida, PrecioNeto = x.PrecioNeto, AfectoIva = x.AfectoIva, EsServicio = x.EsServicio }).SingleOrDefaultAsync(ct);
    public async Task<Resultado> GuardarProductoAsync(ProductoServicioInput input, CancellationToken ct)
    {
        try
        {
            var codigo = input.Codigo.Trim().ToUpperInvariant();
            if (await db.ProductosServicios.AnyAsync(x => x.Codigo == codigo && x.Id != input.Id, ct)) return Resultado.Fallo("Ya existe un producto o servicio con ese código.");
            ProductoServicio entity;
            if (input.Id == 0) { entity = new ProductoServicio(codigo, input.Nombre, input.UnidadMedida, input.PrecioNeto, input.AfectoIva, input.EsServicio); db.ProductosServicios.Add(entity); }
            else entity = await db.ProductosServicios.FindAsync([input.Id], ct) ?? throw new DomainException("El producto no existe.");
            entity.Editar(codigo, input.Nombre, input.Descripcion, input.UnidadMedida, input.PrecioNeto, input.AfectoIva, input.EsServicio);
            await db.SaveChangesAsync(ct); auditoria.Registrar("Guardar", nameof(ProductoServicio), entity.Id.ToString(), new { entity.Codigo, entity.Nombre, entity.PrecioNeto }); await db.SaveChangesAsync(ct);
            return Resultado.Ok();
        }
        catch (DomainException ex) { return Resultado.Fallo(ex.Message); }
    }
    public async Task<Resultado> CambiarEstadoProductoAsync(int id, bool activar, CancellationToken ct)
    {
        var x = await db.ProductosServicios.FindAsync([id], ct); if (x is null) return Resultado.Fallo("El producto no existe.");
        x.CambiarEstado(activar); await db.SaveChangesAsync(ct); return Resultado.Ok();
    }

    public async Task<IReadOnlyList<CotizacionListItemDto>> ListarCotizacionesAsync(CancellationToken ct) =>
        await db.Cotizaciones.AsNoTracking().OrderByDescending(x => x.FechaEmision).ThenByDescending(x => x.Id)
            .Select(x => new CotizacionListItemDto(x.Id, x.Numero, x.FechaEmision, x.FechaVencimiento, x.Cliente.RazonSocial, x.Estado, x.Total)).ToListAsync(ct);
    public async Task<CotizacionDetalleDto?> ObtenerCotizacionAsync(int id, CancellationToken ct) =>
        await db.Cotizaciones.AsNoTracking().Where(x => x.Id == id).Select(x => new CotizacionDetalleDto(x.Id, x.Numero, x.FechaEmision, x.FechaVencimiento,
            x.Estado, x.Observacion, x.Cliente.Rut, x.Cliente.RazonSocial, x.Cliente.Email, x.SubtotalNeto, x.MontoIva, x.Total,
            x.Detalles.OrderBy(d => d.Id).Select(d => new CotizacionDetalleLineaDto(d.ProductoServicio.Codigo, d.Descripcion, d.Cantidad,
                d.ProductoServicio.UnidadMedida, d.PrecioUnitario, d.DescuentoPorcentaje, d.TotalNeto, d.MontoIva, d.Total)).ToList())).SingleOrDefaultAsync(ct);

    public async Task<Resultado<int>> CrearCotizacionAsync(CotizacionInput input, CancellationToken ct)
    {
        if (current.UserId is null) return Resultado<int>.Fallo("Debe iniciar sesión.");
        var empresa = await db.Empresas.AsNoTracking().OrderBy(x => x.Id).FirstOrDefaultAsync(ct);
        var cliente = await db.Clientes.AsNoTracking().SingleOrDefaultAsync(x => x.Id == input.ClienteId && x.Estado == EstadoCatalogo.Activo, ct);
        if (cliente is null) return Resultado<int>.Fallo("Seleccione un cliente activo.");
        var solicitudes = input.Lineas.Where(x => x.ProductoServicioId > 0 && x.Cantidad > 0).ToList();
        if (solicitudes.Count == 0) return Resultado<int>.Fallo("Agregue al menos una línea con cantidad mayor que cero.");
        var ids = solicitudes.Select(x => x.ProductoServicioId).Distinct().ToList();
        var productos = await db.ProductosServicios.Where(x => ids.Contains(x.Id) && x.Estado == EstadoCatalogo.Activo).ToDictionaryAsync(x => x.Id, ct);
        if (productos.Count != ids.Count) return Resultado<int>.Fallo("Uno de los productos no existe o está inactivo.");
        try
        {
            var entity = new Cotizacion($"COT-{clock.UtcNow:yyyyMMdd-HHmmssfff}", input.ClienteId, input.FechaEmision.Date,
                input.FechaVencimiento.Date, current.UserId, input.Observacion);
            foreach (var solicitud in solicitudes)
            {
                var p = productos[solicitud.ProductoServicioId];
                entity.Detalles.Add(new CotizacionDetalle(p.Id, p.Nombre, solicitud.Cantidad, p.PrecioNeto, solicitud.DescuentoPorcentaje, p.AfectoIva, empresa?.IvaPorcentaje ?? 19m));
            }
            entity.EstablecerTotales(entity.Detalles.Sum(x => x.TotalNeto), entity.Detalles.Sum(x => x.MontoIva));
            db.Cotizaciones.Add(entity); await db.SaveChangesAsync(ct);
            auditoria.Registrar("Crear", nameof(Cotizacion), entity.Id.ToString(), new { entity.Numero, entity.ClienteId, entity.Total }); await db.SaveChangesAsync(ct);
            return Resultado<int>.Ok(entity.Id);
        }
        catch (DomainException ex) { return Resultado<int>.Fallo(ex.Message); }
    }
    public async Task<Resultado> CambiarEstadoCotizacionAsync(int id, EstadoCotizacion estado, CancellationToken ct)
    {
        var x = await db.Cotizaciones.FindAsync([id], ct); if (x is null) return Resultado.Fallo("La cotización no existe.");
        try
        {
            if (estado == EstadoCotizacion.Enviada) x.Enviar(); else if (estado == EstadoCotizacion.Aceptada) x.Aceptar();
            else if (estado == EstadoCotizacion.Rechazada) x.Rechazar(); else return Resultado.Fallo("Cambio de estado no permitido.");
            auditoria.Registrar("CambiarEstado", nameof(Cotizacion), x.Id.ToString(), new { Estado = estado }); await db.SaveChangesAsync(ct); return Resultado.Ok();
        }
        catch (DomainException ex) { return Resultado.Fallo(ex.Message); }
    }
    public async Task<Resultado<int>> FacturarCotizacionAsync(int id, CancellationToken ct)
    {
        if (current.UserId is null) return Resultado<int>.Fallo("Debe iniciar sesión.");
        var cotizacion = await db.Cotizaciones.Include(x => x.Detalles).ThenInclude(x => x.ProductoServicio).SingleOrDefaultAsync(x => x.Id == id, ct);
        if (cotizacion is null) return Resultado<int>.Fallo("La cotización no existe.");
        if (await db.Facturas.AnyAsync(x => x.CotizacionId == id, ct)) return Resultado<int>.Fallo("La cotización ya fue facturada.");
        try
        {
            cotizacion.MarcarFacturada();
            var factura = new Factura($"FAC-{clock.UtcNow:yyyyMMdd-HHmmssfff}", cotizacion.Id, cotizacion.ClienteId, clock.UtcNow.Date,
                clock.UtcNow.Date.AddDays(30), cotizacion.SubtotalNeto, cotizacion.MontoIva, current.UserId);
            foreach (var d in cotizacion.Detalles)
                factura.Detalles.Add(new FacturaDetalle(d.ProductoServicio.Codigo, d.Descripcion, d.ProductoServicio.UnidadMedida,
                    d.Cantidad, d.PrecioUnitario, d.DescuentoPorcentaje, d.TotalNeto, d.MontoIva, d.Total));
            db.Facturas.Add(factura); await db.SaveChangesAsync(ct);
            auditoria.Registrar("Emitir", nameof(Factura), factura.Id.ToString(), new { factura.Numero, factura.Total, Cotizacion = cotizacion.Numero }); await db.SaveChangesAsync(ct);
            return Resultado<int>.Ok(factura.Id);
        }
        catch (DomainException ex) { return Resultado<int>.Fallo(ex.Message); }
    }

    public async Task<IReadOnlyList<FacturaListItemDto>> ListarFacturasAsync(CancellationToken ct) =>
        await db.Facturas.AsNoTracking().OrderByDescending(x => x.FechaEmision).ThenByDescending(x => x.Id)
            .Select(x => new FacturaListItemDto(x.Id, x.Numero, x.FechaEmision, x.FechaVencimiento, x.Cliente.RazonSocial, x.Estado, x.Total)).ToListAsync(ct);
    public async Task<FacturaDetalleDto?> ObtenerFacturaAsync(int id, CancellationToken ct) =>
        await db.Facturas.AsNoTracking().Where(x => x.Id == id).Select(x => new FacturaDetalleDto(x.Id, x.Numero, x.FechaEmision, x.FechaVencimiento,
            x.Estado, x.Cliente.Rut, x.Cliente.RazonSocial, x.Cliente.Email, x.SubtotalNeto, x.MontoIva, x.Total, x.FechaPago, x.ReferenciaPago,
            x.Cotizacion.Numero, x.Detalles.OrderBy(d => d.Id).Select(d => new CotizacionDetalleLineaDto(d.Codigo, d.Descripcion, d.Cantidad,
                d.UnidadMedida, d.PrecioUnitario, d.DescuentoPorcentaje, d.TotalNeto, d.MontoIva, d.Total)).ToList())).SingleOrDefaultAsync(ct);
    public async Task<Resultado> MarcarFacturaPagadaAsync(int id, DateTime fecha, string? referencia, CancellationToken ct)
    {
        var x = await db.Facturas.FindAsync([id], ct); if (x is null) return Resultado.Fallo("La factura no existe.");
        try { x.MarcarPagada(fecha, referencia); auditoria.Registrar("RegistrarPago", nameof(Factura), x.Id.ToString(), new { fecha, referencia }); await db.SaveChangesAsync(ct); return Resultado.Ok(); }
        catch (DomainException ex) { return Resultado.Fallo(ex.Message); }
    }
    public async Task<AdministracionDashboardDto> ObtenerDashboardAdministracionAsync(CancellationToken ct)
    {
        var inicioMes = new DateTime(clock.UtcNow.Year, clock.UtcNow.Month, 1);
        return new(await db.Empresas.Select(x => x.NombreFantasia).FirstOrDefaultAsync(ct) ?? "SG-Operaciones",
            await db.Trabajadores.CountAsync(x => x.Estado == EstadoCatalogo.Activo, ct), await db.Users.CountAsync(x => x.Activo, ct),
            await db.Clientes.CountAsync(x => x.Estado == EstadoCatalogo.Activo, ct), await db.ProductosServicios.CountAsync(x => x.Estado == EstadoCatalogo.Activo, ct),
            await db.Cotizaciones.CountAsync(x => x.Estado == EstadoCotizacion.Enviada || x.Estado == EstadoCotizacion.Aceptada, ct),
            await db.Cotizaciones.Where(x => x.Estado == EstadoCotizacion.Enviada || x.Estado == EstadoCotizacion.Aceptada).SumAsync(x => (decimal?)x.Total, ct) ?? 0,
            await db.Facturas.CountAsync(x => x.Estado == EstadoFactura.Emitida, ct),
            await db.Facturas.Where(x => x.Estado == EstadoFactura.Emitida).SumAsync(x => (decimal?)x.Total, ct) ?? 0,
            await db.Facturas.Where(x => x.FechaEmision >= inicioMes && x.Estado != EstadoFactura.Anulada).SumAsync(x => (decimal?)x.Total, ct) ?? 0);
    }
}
