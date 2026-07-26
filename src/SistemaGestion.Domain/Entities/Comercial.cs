using SistemaGestion.Domain.Enums;
using SistemaGestion.Domain.Exceptions;

namespace SistemaGestion.Domain.Entities;

public sealed class Empresa : EntidadBase
{
    private Empresa() { }
    public Empresa(string razonSocial, string nombreFantasia, string rut)
    {
        RazonSocial = Requerido(razonSocial, "La razón social es obligatoria.");
        NombreFantasia = Requerido(nombreFantasia, "El nombre de fantasía es obligatorio.");
        Rut = Requerido(rut, "El RUT es obligatorio.").ToUpperInvariant();
    }
    public string RazonSocial { get; private set; } = "";
    public string NombreFantasia { get; private set; } = "";
    public string Rut { get; private set; } = "";
    public string? Giro { get; private set; }
    public string? Direccion { get; private set; }
    public string? Comuna { get; private set; }
    public string? Ciudad { get; private set; }
    public string? Email { get; private set; }
    public string? Telefono { get; private set; }
    public string? SitioWeb { get; private set; }
    public string Moneda { get; private set; } = "CLP";
    public decimal IvaPorcentaje { get; private set; } = 19m;
    public void Editar(string razonSocial, string nombreFantasia, string rut, string? giro, string? direccion,
        string? comuna, string? ciudad, string? email, string? telefono, string? sitioWeb, decimal iva)
    {
        if (iva is < 0 or > 100) throw new DomainException("El IVA debe estar entre 0 y 100.");
        RazonSocial = Requerido(razonSocial, "La razón social es obligatoria.");
        NombreFantasia = Requerido(nombreFantasia, "El nombre de fantasía es obligatorio.");
        Rut = Requerido(rut, "El RUT es obligatorio.").ToUpperInvariant();
        Giro = Limpiar(giro); Direccion = Limpiar(direccion); Comuna = Limpiar(comuna); Ciudad = Limpiar(ciudad);
        Email = Limpiar(email); Telefono = Limpiar(telefono); SitioWeb = Limpiar(sitioWeb);
        IvaPorcentaje = iva; FechaModificacion = DateTime.UtcNow;
    }
    private static string Requerido(string value, string message) => string.IsNullOrWhiteSpace(value) ? throw new DomainException(message) : value.Trim();
    private static string? Limpiar(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class Cliente : EntidadBase
{
    private Cliente() { }
    public Cliente(string rut, string razonSocial)
    {
        Rut = Requerido(rut, "El RUT del cliente es obligatorio.").ToUpperInvariant();
        RazonSocial = Requerido(razonSocial, "La razón social del cliente es obligatoria.");
    }
    public string Rut { get; private set; } = "";
    public string RazonSocial { get; private set; } = "";
    public string? NombreContacto { get; private set; }
    public string? Email { get; private set; }
    public string? Telefono { get; private set; }
    public string? Direccion { get; private set; }
    public string? Comuna { get; private set; }
    public string? Ciudad { get; private set; }
    public EstadoCatalogo Estado { get; private set; } = EstadoCatalogo.Activo;
    public void Editar(string rut, string razonSocial, string? contacto, string? email, string? telefono, string? direccion, string? comuna, string? ciudad)
    {
        Rut = Requerido(rut, "El RUT del cliente es obligatorio.").ToUpperInvariant();
        RazonSocial = Requerido(razonSocial, "La razón social del cliente es obligatoria.");
        NombreContacto = Limpiar(contacto); Email = Limpiar(email); Telefono = Limpiar(telefono);
        Direccion = Limpiar(direccion); Comuna = Limpiar(comuna); Ciudad = Limpiar(ciudad); FechaModificacion = DateTime.UtcNow;
    }
    public void CambiarEstado(bool activo) { Estado = activo ? EstadoCatalogo.Activo : EstadoCatalogo.Inactivo; FechaModificacion = DateTime.UtcNow; }
    private static string Requerido(string value, string message) => string.IsNullOrWhiteSpace(value) ? throw new DomainException(message) : value.Trim();
    private static string? Limpiar(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class ProductoServicio : EntidadBase
{
    private ProductoServicio() { }
    public ProductoServicio(string codigo, string nombre, string unidadMedida, decimal precioNeto, bool afectoIva, bool esServicio)
    {
        Editar(codigo, nombre, null, unidadMedida, precioNeto, afectoIva, esServicio);
        FechaModificacion = null;
    }
    public string Codigo { get; private set; } = "";
    public string Nombre { get; private set; } = "";
    public string? Descripcion { get; private set; }
    public string UnidadMedida { get; private set; } = "";
    public decimal PrecioNeto { get; private set; }
    public bool AfectoIva { get; private set; }
    public bool EsServicio { get; private set; }
    public EstadoCatalogo Estado { get; private set; } = EstadoCatalogo.Activo;
    public void Editar(string codigo, string nombre, string? descripcion, string unidadMedida, decimal precioNeto, bool afectoIva, bool esServicio)
    {
        if (precioNeto < 0) throw new DomainException("El precio no puede ser negativo.");
        Codigo = Requerido(codigo, "El código es obligatorio.").ToUpperInvariant();
        Nombre = Requerido(nombre, "El nombre es obligatorio.");
        UnidadMedida = Requerido(unidadMedida, "La unidad de medida es obligatoria.");
        Descripcion = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim();
        PrecioNeto = precioNeto; AfectoIva = afectoIva; EsServicio = esServicio; FechaModificacion = DateTime.UtcNow;
    }
    public void CambiarEstado(bool activo) { Estado = activo ? EstadoCatalogo.Activo : EstadoCatalogo.Inactivo; FechaModificacion = DateTime.UtcNow; }
    private static string Requerido(string value, string message) => string.IsNullOrWhiteSpace(value) ? throw new DomainException(message) : value.Trim();
}

public sealed class Cotizacion : EntidadBase
{
    private Cotizacion() { }
    public Cotizacion(string numero, int clienteId, DateTime fechaEmision, DateTime fechaVencimiento, string usuarioId, string? observacion)
    {
        if (clienteId <= 0) throw new DomainException("El cliente es obligatorio.");
        if (fechaVencimiento.Date < fechaEmision.Date) throw new DomainException("El vencimiento no puede ser anterior a la emisión.");
        Numero = numero; ClienteId = clienteId; FechaEmision = fechaEmision; FechaVencimiento = fechaVencimiento;
        UsuarioResponsableId = usuarioId; Observacion = string.IsNullOrWhiteSpace(observacion) ? null : observacion.Trim();
    }
    public string Numero { get; private set; } = "";
    public int ClienteId { get; private set; }
    public Cliente Cliente { get; private set; } = null!;
    public DateTime FechaEmision { get; private set; }
    public DateTime FechaVencimiento { get; private set; }
    public EstadoCotizacion Estado { get; private set; } = EstadoCotizacion.Borrador;
    public string? Observacion { get; private set; }
    public decimal SubtotalNeto { get; private set; }
    public decimal MontoIva { get; private set; }
    public decimal Total { get; private set; }
    public string UsuarioResponsableId { get; private set; } = "";
    public ICollection<CotizacionDetalle> Detalles { get; private set; } = [];
    public void EstablecerTotales(decimal neto, decimal iva) { SubtotalNeto = neto; MontoIva = iva; Total = neto + iva; }
    public void Enviar() { if (Estado != EstadoCotizacion.Borrador) throw new DomainException("Solo una cotización en borrador puede enviarse."); Estado = EstadoCotizacion.Enviada; FechaModificacion = DateTime.UtcNow; }
    public void Aceptar() { if (Estado is not (EstadoCotizacion.Borrador or EstadoCotizacion.Enviada)) throw new DomainException("La cotización no puede aceptarse en su estado actual."); Estado = EstadoCotizacion.Aceptada; FechaModificacion = DateTime.UtcNow; }
    public void Rechazar() { if (Estado is EstadoCotizacion.Facturada or EstadoCotizacion.Anulada) throw new DomainException("La cotización no puede rechazarse."); Estado = EstadoCotizacion.Rechazada; FechaModificacion = DateTime.UtcNow; }
    public void MarcarFacturada() { if (Estado != EstadoCotizacion.Aceptada) throw new DomainException("Debe aceptar la cotización antes de facturar."); Estado = EstadoCotizacion.Facturada; FechaModificacion = DateTime.UtcNow; }
}

public sealed class CotizacionDetalle
{
    private CotizacionDetalle() { }
    public CotizacionDetalle(int productoServicioId, string descripcion, decimal cantidad, decimal precioUnitario, decimal descuentoPorcentaje, bool afectoIva, decimal ivaPorcentaje)
    {
        if (cantidad <= 0) throw new DomainException("La cantidad debe ser mayor que cero.");
        if (precioUnitario < 0 || descuentoPorcentaje is < 0 or > 100) throw new DomainException("Precio o descuento no válido.");
        ProductoServicioId = productoServicioId; Descripcion = descripcion; Cantidad = cantidad; PrecioUnitario = precioUnitario;
        DescuentoPorcentaje = descuentoPorcentaje; AfectoIva = afectoIva;
        TotalNeto = decimal.Round(cantidad * precioUnitario * (1 - descuentoPorcentaje / 100), 2);
        MontoIva = afectoIva ? decimal.Round(TotalNeto * ivaPorcentaje / 100, 2) : 0;
        Total = TotalNeto + MontoIva;
    }
    public int Id { get; private set; }
    public int CotizacionId { get; private set; }
    public Cotizacion Cotizacion { get; private set; } = null!;
    public int ProductoServicioId { get; private set; }
    public ProductoServicio ProductoServicio { get; private set; } = null!;
    public string Descripcion { get; private set; } = "";
    public decimal Cantidad { get; private set; }
    public decimal PrecioUnitario { get; private set; }
    public decimal DescuentoPorcentaje { get; private set; }
    public bool AfectoIva { get; private set; }
    public decimal TotalNeto { get; private set; }
    public decimal MontoIva { get; private set; }
    public decimal Total { get; private set; }
}

public sealed class Factura : EntidadBase
{
    private Factura() { }
    public Factura(string numero, int cotizacionId, int clienteId, DateTime fechaEmision, DateTime fechaVencimiento,
        decimal neto, decimal iva, string usuarioId)
    {
        Numero = numero; CotizacionId = cotizacionId; ClienteId = clienteId; FechaEmision = fechaEmision;
        FechaVencimiento = fechaVencimiento; SubtotalNeto = neto; MontoIva = iva; Total = neto + iva; UsuarioResponsableId = usuarioId;
    }
    public string Numero { get; private set; } = "";
    public int CotizacionId { get; private set; }
    public Cotizacion Cotizacion { get; private set; } = null!;
    public int ClienteId { get; private set; }
    public Cliente Cliente { get; private set; } = null!;
    public DateTime FechaEmision { get; private set; }
    public DateTime FechaVencimiento { get; private set; }
    public EstadoFactura Estado { get; private set; } = EstadoFactura.Emitida;
    public decimal SubtotalNeto { get; private set; }
    public decimal MontoIva { get; private set; }
    public decimal Total { get; private set; }
    public DateTime? FechaPago { get; private set; }
    public string? ReferenciaPago { get; private set; }
    public string UsuarioResponsableId { get; private set; } = "";
    public ICollection<FacturaDetalle> Detalles { get; private set; } = [];
    public void MarcarPagada(DateTime fecha, string? referencia) { if (Estado != EstadoFactura.Emitida) throw new DomainException("Solo una factura emitida puede pagarse."); Estado = EstadoFactura.Pagada; FechaPago = fecha; ReferenciaPago = referencia?.Trim(); FechaModificacion = DateTime.UtcNow; }
}

public sealed class FacturaDetalle
{
    private FacturaDetalle() { }
    public FacturaDetalle(string codigo, string descripcion, string unidadMedida, decimal cantidad, decimal precioUnitario, decimal descuento, decimal neto, decimal iva, decimal total)
    { Codigo = codigo; Descripcion = descripcion; UnidadMedida = unidadMedida; Cantidad = cantidad; PrecioUnitario = precioUnitario; DescuentoPorcentaje = descuento; TotalNeto = neto; MontoIva = iva; Total = total; }
    public int Id { get; private set; }
    public int FacturaId { get; private set; }
    public Factura Factura { get; private set; } = null!;
    public string Codigo { get; private set; } = "";
    public string Descripcion { get; private set; } = "";
    public string UnidadMedida { get; private set; } = "";
    public decimal Cantidad { get; private set; }
    public decimal PrecioUnitario { get; private set; }
    public decimal DescuentoPorcentaje { get; private set; }
    public decimal TotalNeto { get; private set; }
    public decimal MontoIva { get; private set; }
    public decimal Total { get; private set; }
}
