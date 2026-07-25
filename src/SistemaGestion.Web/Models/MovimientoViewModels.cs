using SistemaGestion.Application.Common;
using SistemaGestion.Application.DTOs;

namespace SistemaGestion.Web.Models;

public sealed record EntradaIndexViewModel(EntradaFiltro Filtro, PagedResult<EntradaListItemDto> Resultado, IReadOnlyList<TipoRegistroDto> Tipos);
public sealed record EntradaCrearViewModel(EntradaInput Input, IReadOnlyList<TipoRegistroDto> Tipos);
public sealed record AsignacionIndexViewModel(AsignacionFiltro Filtro, PagedResult<AsignacionListItemDto> Resultado, IReadOnlyList<TrabajadorDto> Trabajadores);
public sealed record AsignacionCrearViewModel(AsignacionInput Input, IReadOnlyList<EntradaOpcionDto> Entradas, IReadOnlyList<TrabajadorDto> Trabajadores);
public sealed record MermaIndexViewModel(MermaFiltro Filtro, PagedResult<MermaListItemDto> Resultado, IReadOnlyList<MotivoMermaDto> Motivos);
public sealed record MermaCrearViewModel(MermaInput Input, IReadOnlyList<EntradaOpcionDto> Entradas, IReadOnlyList<MotivoMermaDto> Motivos);
public sealed record IndicadorMermaViewModel(IndicadorMermaDto Indicador, IReadOnlyList<TipoRegistroDto> Tipos);
public sealed record AnulacionViewModel(AnulacionDetalleDto Detalle, AnulacionInput Input);
public sealed record InformeTiposViewModel(InformeTipoDto Informe);
public sealed record InformeTrabajadorViewModel(
    InformeTrabajadorFiltro Filtro, InformeTrabajadorDto? Informe, IReadOnlyList<TrabajadorDto> Trabajadores);
public sealed record ConsultaMovimientosViewModel(
    ConsultaMovimientoFiltro Filtro, PagedResult<MovimientoConsultaItemDto> Resultado,
    IReadOnlyList<TrabajadorDto> Trabajadores, IReadOnlyList<TipoRegistroDto> Tipos);
