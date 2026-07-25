using System.ComponentModel.DataAnnotations;
using SistemaGestion.Application.Common;
using SistemaGestion.Domain.Enums;

namespace SistemaGestion.Application.DTOs;

public sealed class EntradaFiltro
{
    [DataType(DataType.Date), Display(Name = "Desde")] public DateTime? FechaDesde { get; set; }
    [DataType(DataType.Date), Display(Name = "Hasta")] public DateTime? FechaHasta { get; set; }
    [Display(Name = "Tipo")] public int? TipoRegistroId { get; set; }
    [StringLength(100), Display(Name = "Documento")] public string? DocumentoOrigen { get; set; }
    public EstadoMovimiento? Estado { get; set; }
    [Range(1, int.MaxValue)] public int Pagina { get; set; } = 1;
    [Range(5, 100)] public int TamanoPagina { get; set; } = 20;
}

public sealed class EntradaInput
{
    [Required, Display(Name = "Tipo de registro")] public int TipoRegistroId { get; set; }
    [Required, Display(Name = "Fecha y hora")] public DateTime FechaHora { get; set; } = DateTime.Now;
    [Range(typeof(decimal), "0.001", "999999999999999.999"), Display(Name = "Cantidad inicial")] public decimal CantidadInicial { get; set; }
    [Required, StringLength(100), Display(Name = "Documento de origen")] public string DocumentoOrigen { get; set; } = "";
    [StringLength(1000), Display(Name = "Observación")] public string? Observacion { get; set; }
}

public sealed record EntradaListItemDto(
    int Id, DateTime FechaHora, string Tipo, string UnidadMedida, decimal CantidadInicial,
    decimal TotalAsignado, decimal TotalMerma, decimal SaldoDisponible, string DocumentoOrigen,
    EstadoMovimiento Estado, string UsuarioResponsable, byte[] RowVersion);

public sealed record EntradaDetalleDto(
    int Id, DateTime FechaHora, string Tipo, string UnidadMedida, decimal CantidadInicial,
    decimal TotalAsignado, decimal TotalMerma, decimal SaldoDisponible, string DocumentoOrigen,
    string? Observacion, EstadoMovimiento Estado, string UsuarioResponsable, byte[] RowVersion,
    IReadOnlyList<AsignacionListItemDto> Asignaciones, IReadOnlyList<MermaListItemDto> Mermas);

public sealed record EntradaOpcionDto(int Id, string Etiqueta, string Tipo, string UnidadMedida, decimal SaldoDisponible, byte[] RowVersion);
public sealed record SaldoDto(int EntradaId, decimal CantidadInicial, decimal TotalAsignado, decimal TotalMerma, decimal Disponible, byte[] RowVersion);

public sealed class AsignacionFiltro
{
    [DataType(DataType.Date), Display(Name = "Desde")] public DateTime? FechaDesde { get; set; }
    [DataType(DataType.Date), Display(Name = "Hasta")] public DateTime? FechaHasta { get; set; }
    [Display(Name = "Trabajador")] public int? TrabajadorId { get; set; }
    [Display(Name = "Entrada")] public int? EntradaId { get; set; }
    public EstadoMovimiento? Estado { get; set; }
    [Range(1, int.MaxValue)] public int Pagina { get; set; } = 1;
    [Range(5, 100)] public int TamanoPagina { get; set; } = 20;
}

public sealed class AsignacionInput
{
    [Required, Display(Name = "Entrada de origen")] public int EntradaId { get; set; }
    [Required, Display(Name = "Trabajador")] public int TrabajadorId { get; set; }
    [Required, Display(Name = "Fecha y hora")] public DateTime FechaHora { get; set; } = DateTime.Now;
    [Range(typeof(decimal), "0.001", "999999999999999.999")] public decimal Cantidad { get; set; }
    [StringLength(1000), Display(Name = "Observación")] public string? Observacion { get; set; }
    [Required] public string EntradaRowVersion { get; set; } = "";
}

public sealed record AsignacionListItemDto(
    int Id, DateTime FechaHora, int EntradaId, string DocumentoOrigen, string Tipo, string UnidadMedida,
    int TrabajadorId, string Trabajador, decimal Cantidad, EstadoMovimiento Estado, string UsuarioResponsable);

public sealed class MermaFiltro
{
    [DataType(DataType.Date), Display(Name = "Desde")] public DateTime? FechaDesde { get; set; }
    [DataType(DataType.Date), Display(Name = "Hasta")] public DateTime? FechaHasta { get; set; }
    [Display(Name = "Motivo")] public int? MotivoMermaId { get; set; }
    [Display(Name = "Tipo")] public int? TipoRegistroId { get; set; }
    [Display(Name = "Entrada")] public int? EntradaId { get; set; }
    public EstadoMovimiento? Estado { get; set; }
    [Range(1, int.MaxValue)] public int Pagina { get; set; } = 1;
    [Range(5, 100)] public int TamanoPagina { get; set; } = 20;
}

public sealed class MermaInput
{
    [Required, Display(Name = "Entrada de origen")] public int EntradaId { get; set; }
    [Required, Display(Name = "Motivo de merma")] public int MotivoMermaId { get; set; }
    [Required, Display(Name = "Fecha y hora")] public DateTime FechaHora { get; set; } = DateTime.Now;
    [Range(typeof(decimal), "0.001", "999999999999999.999")] public decimal Cantidad { get; set; }
    [StringLength(1000), Display(Name = "Observación")] public string? Observacion { get; set; }
    [StringLength(500), Display(Name = "Referencia de evidencia")] public string? EvidenciaReferencia { get; set; }
    [Required] public string EntradaRowVersion { get; set; } = "";
}

public sealed record MermaListItemDto(
    int Id, DateTime FechaHora, int EntradaId, string DocumentoOrigen, string Tipo, string UnidadMedida,
    int MotivoMermaId, string Motivo, decimal Cantidad, string? EvidenciaReferencia,
    EstadoMovimiento Estado, string UsuarioResponsable);

public sealed class IndicadorMermaFiltro
{
    [Required, DataType(DataType.Date), Display(Name = "Desde")] public DateTime FechaDesde { get; set; } = DateTime.Today.AddMonths(-1);
    [Required, DataType(DataType.Date), Display(Name = "Hasta")] public DateTime FechaHasta { get; set; } = DateTime.Today;
    [Display(Name = "Tipo de registro")] public int? TipoRegistroId { get; set; }
}

public sealed record IndicadorMermaItemDto(
    int MotivoMermaId, int TipoRegistroId, string Motivo, string Tipo, string UnidadMedida, decimal Cantidad, int Frecuencia,
    decimal PorcentajeMermas, decimal PorcentajeEntradas, decimal PorcentajeAcumulado);

public sealed record IndicadorMermaDto(
    IndicadorMermaFiltro Filtro, decimal TotalMermas, decimal TotalEntradas,
    IReadOnlyList<IndicadorMermaItemDto> Items);

public sealed class AnulacionInput
{
    public int Id { get; set; }
    public ClaseMovimiento Clase { get; set; }
    [Required, StringLength(500), Display(Name = "Motivo de anulación")] public string Motivo { get; set; } = "";
    [Required] public string RowVersion { get; set; } = "";
}

public sealed record AnulacionDetalleDto(
    int Id, ClaseMovimiento Clase, string Identificador, DateTime FechaHora, decimal Cantidad,
    string UnidadMedida, EstadoMovimiento Estado, byte[] RowVersion);

public sealed record MovimientoPage<TFiltro, TItem>(TFiltro Filtro, PagedResult<TItem> Resultado);
