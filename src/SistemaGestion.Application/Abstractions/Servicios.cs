using SistemaGestion.Application.Common;
using SistemaGestion.Application.DTOs;
using SistemaGestion.Domain.Enums;

namespace SistemaGestion.Application.Abstractions;

public interface ICurrentUserService
{
    string? UserId { get; }
    string UserName { get; }
    string CorrelationId { get; }
    string? IpAddress { get; }
    string? UserAgent { get; }
}
public interface IDateTimeProvider { DateTime UtcNow { get; } }

public interface ITrabajadorService
{
    Task<IReadOnlyList<TrabajadorDto>> ListarAsync(CancellationToken cancellationToken);
    Task<TrabajadorDto?> ObtenerAsync(int id, CancellationToken cancellationToken);
    Task<Resultado> GuardarAsync(TrabajadorInput input, CancellationToken cancellationToken);
    Task<Resultado> CambiarEstadoAsync(int id, bool activar, CancellationToken cancellationToken);
}
public interface ITipoRegistroService
{
    Task<IReadOnlyList<TipoRegistroDto>> ListarAsync(CancellationToken cancellationToken);
    Task<TipoRegistroDto?> ObtenerAsync(int id, CancellationToken cancellationToken);
    Task<Resultado> GuardarAsync(TipoRegistroInput input, CancellationToken cancellationToken);
    Task<Resultado> CambiarEstadoAsync(int id, bool activar, CancellationToken cancellationToken);
}
public interface IMotivoMermaService
{
    Task<IReadOnlyList<MotivoMermaDto>> ListarAsync(CancellationToken cancellationToken);
    Task<MotivoMermaDto?> ObtenerAsync(int id, CancellationToken cancellationToken);
    Task<Resultado> GuardarAsync(MotivoMermaInput input, CancellationToken cancellationToken);
    Task<Resultado> CambiarEstadoAsync(int id, bool activar, CancellationToken cancellationToken);
}

// Contratos de los siguientes incrementos, definidos desde la arquitectura inicial.
public interface IEntradaService
{
    Task<PagedResult<EntradaListItemDto>> ListarAsync(EntradaFiltro filtro, CancellationToken cancellationToken);
    Task<EntradaDetalleDto?> ObtenerDetalleAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyList<EntradaOpcionDto>> ListarDisponiblesAsync(CancellationToken cancellationToken);
    Task<Resultado<int>> CrearAsync(EntradaInput input, CancellationToken cancellationToken);
}
public interface IAsignacionService
{
    Task<PagedResult<AsignacionListItemDto>> ListarAsync(AsignacionFiltro filtro, CancellationToken cancellationToken);
    Task<Resultado<int>> CrearAsync(AsignacionInput input, CancellationToken cancellationToken);
}
public interface IMermaService
{
    Task<PagedResult<MermaListItemDto>> ListarAsync(MermaFiltro filtro, CancellationToken cancellationToken);
    Task<Resultado<int>> CrearAsync(MermaInput input, CancellationToken cancellationToken);
    Task<IndicadorMermaDto> ObtenerIndicadoresAsync(IndicadorMermaFiltro filtro, CancellationToken cancellationToken);
}
public interface IAnulacionService
{
    Task<AnulacionDetalleDto?> ObtenerAsync(ClaseMovimiento clase, int id, CancellationToken cancellationToken);
    Task<Resultado> AnularAsync(AnulacionInput input, CancellationToken cancellationToken);
}
public interface ISaldoService
{
    Task<SaldoDto?> ObtenerAsync(int entradaId, CancellationToken cancellationToken);
}
public interface IReporteService
{
    Task<DashboardDto> ObtenerDashboardAsync(DashboardFiltro filtro, CancellationToken cancellationToken);
    Task<PagedResult<MovimientoConsultaItemDto>> ConsultarMovimientosAsync(ConsultaMovimientoFiltro filtro, CancellationToken cancellationToken);
    Task<InformeTrabajadorDto?> ObtenerInformeTrabajadorAsync(InformeTrabajadorFiltro filtro, CancellationToken cancellationToken);
    Task<InformeTipoDto> ObtenerInformeTiposAsync(InformePeriodoFiltro filtro, CancellationToken cancellationToken);
}
public interface IAuditoriaConsultaService
{
    Task<PagedResult<AuditoriaItemDto>> ListarAsync(AuditoriaFiltro filtro, CancellationToken cancellationToken);
}
public interface IAuditoriaService
{
    void Registrar(string accion, string entidad, string clavePrimaria, object? valoresNuevos = null, object? valoresAnteriores = null, string? motivo = null);
    Task RegistrarYGuardarAsync(string accion, string entidad, string clavePrimaria, object? detalle, CancellationToken cancellationToken);
}
public interface IExportacionService
{
    ArchivoExportado IndicadoresCsv(IndicadorMermaDto indicador);
    ArchivoExportado IndicadoresPdf(IndicadorMermaDto indicador);
    ArchivoExportado InformeTrabajadorCsv(InformeTrabajadorDto informe);
    ArchivoExportado InformeTrabajadorPdf(InformeTrabajadorDto informe);
    ArchivoExportado InformeTiposCsv(InformeTipoDto informe);
    ArchivoExportado InformeTiposPdf(InformeTipoDto informe);
}
