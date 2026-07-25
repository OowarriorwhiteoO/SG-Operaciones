using System.ComponentModel.DataAnnotations;
using SistemaGestion.Domain.Enums;

namespace SistemaGestion.Application.DTOs;

public sealed class ConsultaMovimientoFiltro
{
    [DataType(DataType.Date), Display(Name = "Desde")] public DateTime? FechaDesde { get; set; }
    [DataType(DataType.Date), Display(Name = "Hasta")] public DateTime? FechaHasta { get; set; }
    [Display(Name = "Trabajador")] public int? TrabajadorId { get; set; }
    [Display(Name = "Tipo")] public int? TipoRegistroId { get; set; }
    [Display(Name = "Movimiento")] public ClaseMovimiento? Clase { get; set; }
    public EstadoMovimiento? Estado { get; set; }
    [StringLength(100), Display(Name = "Documento")] public string? DocumentoOrigen { get; set; }
    [StringLength(150), Display(Name = "Usuario responsable")] public string? UsuarioResponsable { get; set; }
    [Range(1, int.MaxValue)] public int Pagina { get; set; } = 1;
    [Range(5, 100)] public int TamanoPagina { get; set; } = 20;
}

public sealed class MovimientoConsultaItemDto
{
    public int Id { get; init; }
    public ClaseMovimiento Clase { get; init; }
    public DateTime FechaHora { get; init; }
    public int EntradaId { get; init; }
    public string DocumentoOrigen { get; init; } = "";
    public string Tipo { get; init; } = "";
    public string UnidadMedida { get; init; } = "";
    public decimal Cantidad { get; init; }
    public string? Trabajador { get; init; }
    public string? Detalle { get; init; }
    public EstadoMovimiento Estado { get; init; }
    public string UsuarioResponsable { get; init; } = "";
}

public class InformePeriodoFiltro
{
    [Required, DataType(DataType.Date), Display(Name = "Desde")]
    public DateTime FechaDesde { get; set; } = DateTime.Today.AddMonths(-1);

    [Required, DataType(DataType.Date), Display(Name = "Hasta")]
    public DateTime FechaHasta { get; set; } = DateTime.Today;
}

public sealed class InformeTrabajadorFiltro : InformePeriodoFiltro
{
    [Required, Display(Name = "Trabajador")]
    public int TrabajadorId { get; set; }

    [Display(Name = "Incluir asignaciones anuladas")]
    public bool IncluirAnuladas { get; set; }
}

public sealed record InformeTrabajadorItemDto(
    int AsignacionId, DateTime FechaHora, int EntradaId, string DocumentoOrigen,
    string Tipo, string UnidadMedida, decimal Cantidad, EstadoMovimiento Estado);

public sealed record InformeTrabajadorSubtotalDto(string Tipo, string UnidadMedida, decimal Cantidad);

public sealed record InformeTrabajadorDto(
    InformeTrabajadorFiltro Filtro, int TrabajadorId, string Rut, string Trabajador,
    string Area, EstadoCatalogo Estado, IReadOnlyList<InformeTrabajadorItemDto> Items,
    IReadOnlyList<InformeTrabajadorSubtotalDto> Subtotales, decimal Total,
    DateTime GeneradoUtc, string GeneradoPor);

public sealed record InformeTipoItemDto(
    int TipoRegistroId, string Tipo, string UnidadMedida, decimal Entradas,
    int CantidadEntradas, decimal Asignaciones, int CantidadAsignaciones,
    decimal Mermas, int CantidadMermas, decimal Saldo, decimal PorcentajeMerma);

public sealed record InformeTipoDto(
    InformePeriodoFiltro Filtro, IReadOnlyList<InformeTipoItemDto> Items,
    DateTime GeneradoUtc, string GeneradoPor);

public sealed record ArchivoExportado(byte[] Contenido, string TipoContenido, string NombreArchivo);

public sealed class DashboardFiltro : InformePeriodoFiltro;

public sealed record DashboardMotivoDto(string Motivo, decimal Cantidad, decimal Porcentaje);

public sealed record DashboardDto(
    DashboardFiltro Filtro, int TotalEntradas, decimal CantidadRecibida,
    decimal CantidadAsignada, decimal CantidadMerma, decimal SaldoTotal,
    int TrabajadoresActivos, int EntradasSaldoBajo,
    IReadOnlyList<MovimientoConsultaItemDto> UltimosMovimientos,
    IReadOnlyList<DashboardMotivoDto> MermasPorMotivo);

public sealed class AuditoriaFiltro
{
    [DataType(DataType.Date), Display(Name = "Desde")] public DateTime? FechaDesde { get; set; }
    [DataType(DataType.Date), Display(Name = "Hasta")] public DateTime? FechaHasta { get; set; }
    [StringLength(100)] public string? Entidad { get; set; }
    [StringLength(100)] public string? Accion { get; set; }
    [StringLength(150), Display(Name = "Usuario")] public string? Usuario { get; set; }
    [Range(1, int.MaxValue)] public int Pagina { get; set; } = 1;
    [Range(5, 100)] public int TamanoPagina { get; set; } = 25;
}

public sealed record AuditoriaItemDto(
    long Id, DateTime FechaHora, string NombreUsuario, string Accion, string Entidad,
    string ClavePrimaria, string CorrelationId, string? Motivo, string? DireccionIp);
