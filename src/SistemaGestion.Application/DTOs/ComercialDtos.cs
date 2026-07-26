using System.ComponentModel.DataAnnotations;
using SistemaGestion.Domain.Enums;

namespace SistemaGestion.Application.DTOs;

public sealed class EmpresaInput
{
    public int Id { get; set; }
    [Required, StringLength(180), Display(Name = "Razón social")] public string RazonSocial { get; set; } = "";
    [Required, StringLength(150), Display(Name = "Nombre de fantasía")] public string NombreFantasia { get; set; } = "";
    [Required, StringLength(20)] public string Rut { get; set; } = "";
    [StringLength(180)] public string? Giro { get; set; }
    [StringLength(250), Display(Name = "Dirección")] public string? Direccion { get; set; }
    [StringLength(100)] public string? Comuna { get; set; }
    [StringLength(100)] public string? Ciudad { get; set; }
    [EmailAddress, StringLength(150)] public string? Email { get; set; }
    [StringLength(40), Display(Name = "Teléfono")] public string? Telefono { get; set; }
    [Url, StringLength(180), Display(Name = "Sitio web")] public string? SitioWeb { get; set; }
    [Range(0, 100), Display(Name = "IVA (%)")] public decimal IvaPorcentaje { get; set; } = 19m;
}

public sealed class ClienteInput
{
    public int Id { get; set; }
    [Required, StringLength(20)] public string Rut { get; set; } = "";
    [Required, StringLength(180), Display(Name = "Razón social")] public string RazonSocial { get; set; } = "";
    [StringLength(150), Display(Name = "Persona de contacto")] public string? NombreContacto { get; set; }
    [EmailAddress, StringLength(150)] public string? Email { get; set; }
    [StringLength(40), Display(Name = "Teléfono")] public string? Telefono { get; set; }
    [StringLength(250), Display(Name = "Dirección")] public string? Direccion { get; set; }
    [StringLength(100)] public string? Comuna { get; set; }
    [StringLength(100)] public string? Ciudad { get; set; }
}
public sealed record ClienteDto(int Id, string Rut, string RazonSocial, string? NombreContacto, string? Email, string? Telefono, string? Direccion, string? Comuna, string? Ciudad, EstadoCatalogo Estado);

public sealed class ProductoServicioInput
{
    public int Id { get; set; }
    [Required, StringLength(40), Display(Name = "Código")] public string Codigo { get; set; } = "";
    [Required, StringLength(150)] public string Nombre { get; set; } = "";
    [StringLength(500), Display(Name = "Descripción")] public string? Descripcion { get; set; }
    [Required, StringLength(30), Display(Name = "Unidad de medida")] public string UnidadMedida { get; set; } = "unidad";
    [Range(0, 999999999999), Display(Name = "Precio neto")] public decimal PrecioNeto { get; set; }
    [Display(Name = "Afecto a IVA")] public bool AfectoIva { get; set; } = true;
    [Display(Name = "Es servicio")] public bool EsServicio { get; set; }
}
public sealed record ProductoServicioDto(int Id, string Codigo, string Nombre, string? Descripcion, string UnidadMedida, decimal PrecioNeto, bool AfectoIva, bool EsServicio, EstadoCatalogo Estado);

public sealed class CotizacionLineaInput
{
    public int ProductoServicioId { get; set; }
    [Range(0, 999999999)] public decimal Cantidad { get; set; }
    [Range(0, 100), Display(Name = "Descuento (%)")] public decimal DescuentoPorcentaje { get; set; }
}
public sealed class CotizacionInput
{
    [Range(1, int.MaxValue), Display(Name = "Cliente")] public int ClienteId { get; set; }
    [DataType(DataType.Date), Display(Name = "Fecha de emisión")] public DateTime FechaEmision { get; set; } = DateTime.Today;
    [DataType(DataType.Date), Display(Name = "Válida hasta")] public DateTime FechaVencimiento { get; set; } = DateTime.Today.AddDays(15);
    [StringLength(1000)] public string? Observacion { get; set; }
    public List<CotizacionLineaInput> Lineas { get; set; } = Enumerable.Range(0, 6).Select(_ => new CotizacionLineaInput()).ToList();
}
public sealed record CotizacionListItemDto(int Id, string Numero, DateTime FechaEmision, DateTime FechaVencimiento, string Cliente, EstadoCotizacion Estado, decimal Total);
public sealed record CotizacionDetalleLineaDto(string Codigo, string Descripcion, decimal Cantidad, string Unidad, decimal PrecioUnitario, decimal Descuento, decimal Neto, decimal Iva, decimal Total);
public sealed record CotizacionDetalleDto(int Id, string Numero, DateTime FechaEmision, DateTime FechaVencimiento, EstadoCotizacion Estado, string? Observacion,
    string ClienteRut, string Cliente, string? ClienteEmail, decimal Neto, decimal Iva, decimal Total, IReadOnlyList<CotizacionDetalleLineaDto> Lineas);

public sealed record FacturaListItemDto(int Id, string Numero, DateTime FechaEmision, DateTime FechaVencimiento, string Cliente, EstadoFactura Estado, decimal Total);
public sealed record FacturaDetalleDto(int Id, string Numero, DateTime FechaEmision, DateTime FechaVencimiento, EstadoFactura Estado,
    string ClienteRut, string Cliente, string? ClienteEmail, decimal Neto, decimal Iva, decimal Total, DateTime? FechaPago, string? ReferenciaPago,
    string CotizacionNumero, IReadOnlyList<CotizacionDetalleLineaDto> Lineas);

public sealed record AdministracionDashboardDto(string Empresa, int Trabajadores, int Usuarios, int Clientes, int Productos,
    int CotizacionesPendientes, decimal CotizacionesMonto, int FacturasPendientes, decimal PorCobrar, decimal FacturadoMes);
