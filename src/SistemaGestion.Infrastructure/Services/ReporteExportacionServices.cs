using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaGestion.Application.Abstractions;
using SistemaGestion.Application.DTOs;
using SistemaGestion.Domain.Enums;
using SistemaGestion.Domain.Exceptions;
using SistemaGestion.Infrastructure.Persistence;

namespace SistemaGestion.Infrastructure.Services;

public sealed class ReporteService(
    ApplicationDbContext db,
    ICurrentUserService current,
    IDateTimeProvider clock) : IReporteService
{
    public async Task<DashboardDto> ObtenerDashboardAsync(DashboardFiltro filtro, CancellationToken ct)
    {
        // El límite superior exclusivo evita perder movimientos con hora en el último día solicitado.
        ValidarRango(filtro.FechaDesde, filtro.FechaHasta);
        var desde = filtro.FechaDesde.Date;
        var hasta = filtro.FechaHasta.Date.AddDays(1);
        var entradas = db.Entradas.AsNoTracking().Where(x =>
            x.Estado == EstadoMovimiento.Vigente && x.FechaHora >= desde && x.FechaHora < hasta);
        var asignaciones = db.Asignaciones.AsNoTracking().Where(x =>
            x.Estado == EstadoMovimiento.Vigente && x.FechaHora >= desde && x.FechaHora < hasta);
        var mermas = db.Mermas.AsNoTracking().Where(x =>
            x.Estado == EstadoMovimiento.Vigente && x.FechaHora >= desde && x.FechaHora < hasta);

        var totalEntradas = await entradas.CountAsync(ct);
        var recibido = await entradas.SumAsync(x => (decimal?)x.CantidadInicial, ct) ?? 0;
        var asignado = await asignaciones.SumAsync(x => (decimal?)x.Cantidad, ct) ?? 0;
        var mermado = await mermas.SumAsync(x => (decimal?)x.Cantidad, ct) ?? 0;
        var trabajadores = await db.Trabajadores.AsNoTracking().CountAsync(x => x.Estado == EstadoCatalogo.Activo, ct);
        var saldoBajo = await entradas.CountAsync(x =>
            x.CantidadInicial
            - (x.Asignaciones.Where(a => a.Estado == EstadoMovimiento.Vigente).Sum(a => (decimal?)a.Cantidad) ?? 0)
            - (x.Mermas.Where(m => m.Estado == EstadoMovimiento.Vigente).Sum(m => (decimal?)m.Cantidad) ?? 0)
            <= x.CantidadInicial * 0.1m, ct);
        var motivos = await mermas.GroupBy(x => x.MotivoMerma.Nombre)
            .Select(x => new { Motivo = x.Key, Cantidad = x.Sum(y => y.Cantidad) })
            .OrderByDescending(x => x.Cantidad).Take(5).ToListAsync(ct);
        var consulta = await ConsultarMovimientosAsync(new ConsultaMovimientoFiltro
        {
            FechaDesde = filtro.FechaDesde, FechaHasta = filtro.FechaHasta, Pagina = 1, TamanoPagina = 5
        }, ct);
        return new(filtro, totalEntradas, recibido, asignado, mermado, recibido - asignado - mermado,
            trabajadores, saldoBajo, consulta.Items,
            motivos.Select(x => new DashboardMotivoDto(x.Motivo, x.Cantidad,
                mermado == 0 ? 0 : x.Cantidad / mermado * 100)).ToList());
    }

    public async Task<SistemaGestion.Application.Common.PagedResult<MovimientoConsultaItemDto>> ConsultarMovimientosAsync(
        ConsultaMovimientoFiltro filtro, CancellationToken ct)
    {
        if (filtro.FechaDesde.HasValue && filtro.FechaHasta.HasValue)
            ValidarRango(filtro.FechaDesde.Value, filtro.FechaHasta.Value);
        var desde = filtro.FechaDesde?.Date;
        var hasta = filtro.FechaHasta?.Date.AddDays(1);
        var documento = filtro.DocumentoOrigen?.Trim();
        var usuario = filtro.UsuarioResponsable?.Trim();

        var entradas = db.Entradas.AsNoTracking().AsQueryable();
        if (filtro.Clase.HasValue && filtro.Clase != ClaseMovimiento.Entrada || filtro.TrabajadorId.HasValue) entradas = entradas.Where(x => false);
        if (desde.HasValue) entradas = entradas.Where(x => x.FechaHora >= desde);
        if (hasta.HasValue) entradas = entradas.Where(x => x.FechaHora < hasta);
        if (filtro.TipoRegistroId.HasValue) entradas = entradas.Where(x => x.TipoRegistroId == filtro.TipoRegistroId);
        if (filtro.Estado.HasValue) entradas = entradas.Where(x => x.Estado == filtro.Estado);
        if (!string.IsNullOrWhiteSpace(documento)) entradas = entradas.Where(x => x.DocumentoOrigen.Contains(documento));
        if (!string.IsNullOrWhiteSpace(usuario)) entradas = entradas.Where(x =>
            x.UsuarioResponsableId.Contains(usuario) || db.Users.Any(u => u.Id == x.UsuarioResponsableId && u.Email!.Contains(usuario)));
        var qEntradas = entradas.Select(x => new MovimientoConsultaItemDto
        {
            Id = x.Id, Clase = ClaseMovimiento.Entrada, FechaHora = x.FechaHora, EntradaId = x.Id,
            DocumentoOrigen = x.DocumentoOrigen, Tipo = x.TipoRegistro.Nombre, UnidadMedida = x.TipoRegistro.UnidadMedida,
            Cantidad = x.CantidadInicial, Trabajador = null, Detalle = "Entrada de inventario", Estado = x.Estado,
            UsuarioResponsable = db.Users.Where(u => u.Id == x.UsuarioResponsableId).Select(u => u.Email).FirstOrDefault() ?? x.UsuarioResponsableId
        });

        var asignaciones = db.Asignaciones.AsNoTracking().AsQueryable();
        if (filtro.Clase.HasValue && filtro.Clase != ClaseMovimiento.Asignacion) asignaciones = asignaciones.Where(x => false);
        if (desde.HasValue) asignaciones = asignaciones.Where(x => x.FechaHora >= desde);
        if (hasta.HasValue) asignaciones = asignaciones.Where(x => x.FechaHora < hasta);
        if (filtro.TrabajadorId.HasValue) asignaciones = asignaciones.Where(x => x.TrabajadorId == filtro.TrabajadorId);
        if (filtro.TipoRegistroId.HasValue) asignaciones = asignaciones.Where(x => x.Entrada.TipoRegistroId == filtro.TipoRegistroId);
        if (filtro.Estado.HasValue) asignaciones = asignaciones.Where(x => x.Estado == filtro.Estado);
        if (!string.IsNullOrWhiteSpace(documento)) asignaciones = asignaciones.Where(x => x.Entrada.DocumentoOrigen.Contains(documento));
        if (!string.IsNullOrWhiteSpace(usuario)) asignaciones = asignaciones.Where(x =>
            x.UsuarioResponsableId.Contains(usuario) || db.Users.Any(u => u.Id == x.UsuarioResponsableId && u.Email!.Contains(usuario)));
        var qAsignaciones = asignaciones.Select(x => new MovimientoConsultaItemDto
        {
            Id = x.Id, Clase = ClaseMovimiento.Asignacion, FechaHora = x.FechaHora, EntradaId = x.EntradaId,
            DocumentoOrigen = x.Entrada.DocumentoOrigen, Tipo = x.Entrada.TipoRegistro.Nombre, UnidadMedida = x.Entrada.TipoRegistro.UnidadMedida,
            Cantidad = x.Cantidad, Trabajador = x.Trabajador.NombreCompleto, Detalle = "Asignación a trabajador", Estado = x.Estado,
            UsuarioResponsable = db.Users.Where(u => u.Id == x.UsuarioResponsableId).Select(u => u.Email).FirstOrDefault() ?? x.UsuarioResponsableId
        });

        var mermas = db.Mermas.AsNoTracking().AsQueryable();
        if (filtro.Clase.HasValue && filtro.Clase != ClaseMovimiento.Merma || filtro.TrabajadorId.HasValue) mermas = mermas.Where(x => false);
        if (desde.HasValue) mermas = mermas.Where(x => x.FechaHora >= desde);
        if (hasta.HasValue) mermas = mermas.Where(x => x.FechaHora < hasta);
        if (filtro.TipoRegistroId.HasValue) mermas = mermas.Where(x => x.Entrada.TipoRegistroId == filtro.TipoRegistroId);
        if (filtro.Estado.HasValue) mermas = mermas.Where(x => x.Estado == filtro.Estado);
        if (!string.IsNullOrWhiteSpace(documento)) mermas = mermas.Where(x => x.Entrada.DocumentoOrigen.Contains(documento));
        if (!string.IsNullOrWhiteSpace(usuario)) mermas = mermas.Where(x =>
            x.UsuarioResponsableId.Contains(usuario) || db.Users.Any(u => u.Id == x.UsuarioResponsableId && u.Email!.Contains(usuario)));
        var qMermas = mermas.Select(x => new MovimientoConsultaItemDto
        {
            Id = x.Id, Clase = ClaseMovimiento.Merma, FechaHora = x.FechaHora, EntradaId = x.EntradaId,
            DocumentoOrigen = x.Entrada.DocumentoOrigen, Tipo = x.Entrada.TipoRegistro.Nombre, UnidadMedida = x.Entrada.TipoRegistro.UnidadMedida,
            Cantidad = x.Cantidad, Trabajador = null, Detalle = x.MotivoMerma.Nombre, Estado = x.Estado,
            UsuarioResponsable = db.Users.Where(u => u.Id == x.UsuarioResponsableId).Select(u => u.Email).FirstOrDefault() ?? x.UsuarioResponsableId
        });

        var query = qEntradas.Concat(qAsignaciones).Concat(qMermas);
        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(x => x.FechaHora).ThenByDescending(x => x.Id)
            .Skip((filtro.Pagina - 1) * filtro.TamanoPagina).Take(filtro.TamanoPagina).ToListAsync(ct);
        return new(items, filtro.Pagina, filtro.TamanoPagina, total);
    }

    public async Task<InformeTrabajadorDto?> ObtenerInformeTrabajadorAsync(
        InformeTrabajadorFiltro filtro, CancellationToken ct)
    {
        ValidarRango(filtro.FechaDesde, filtro.FechaHasta);
        var trabajador = await db.Trabajadores.AsNoTracking()
            .Where(x => x.Id == filtro.TrabajadorId)
            .Select(x => new { x.Id, x.Rut, x.NombreCompleto, x.Area, x.Estado })
            .SingleOrDefaultAsync(ct);
        if (trabajador is null) return null;

        var hasta = filtro.FechaHasta.Date.AddDays(1);
        var query = db.Asignaciones.AsNoTracking().Where(x =>
            x.TrabajadorId == filtro.TrabajadorId &&
            x.FechaHora >= filtro.FechaDesde.Date && x.FechaHora < hasta);
        if (!filtro.IncluirAnuladas)
            query = query.Where(x => x.Estado == EstadoMovimiento.Vigente);

        var items = await query.OrderByDescending(x => x.FechaHora).ThenByDescending(x => x.Id)
            .Select(x => new InformeTrabajadorItemDto(
                x.Id, x.FechaHora, x.EntradaId, x.Entrada.DocumentoOrigen,
                x.Entrada.TipoRegistro.Nombre, x.Entrada.TipoRegistro.UnidadMedida,
                x.Cantidad, x.Estado))
            .ToListAsync(ct);
        var subtotales = items.Where(x => x.Estado == EstadoMovimiento.Vigente)
            .GroupBy(x => new { x.Tipo, x.UnidadMedida })
            .Select(x => new InformeTrabajadorSubtotalDto(x.Key.Tipo, x.Key.UnidadMedida, x.Sum(y => y.Cantidad)))
            .OrderBy(x => x.Tipo).ToList();
        return new(filtro, trabajador.Id, trabajador.Rut, trabajador.NombreCompleto, trabajador.Area,
            trabajador.Estado, items, subtotales, subtotales.Sum(x => x.Cantidad), clock.UtcNow, current.UserName);
    }

    public async Task<InformeTipoDto> ObtenerInformeTiposAsync(InformePeriodoFiltro filtro, CancellationToken ct)
    {
        ValidarRango(filtro.FechaDesde, filtro.FechaHasta);
        var desde = filtro.FechaDesde.Date;
        var hasta = filtro.FechaHasta.Date.AddDays(1);
        var tipos = await db.TiposRegistro.AsNoTracking()
            .OrderBy(x => x.Nombre).Select(x => new { x.Id, x.Nombre, x.UnidadMedida }).ToListAsync(ct);
        var entradas = await db.Entradas.AsNoTracking()
            .Where(x => x.Estado == EstadoMovimiento.Vigente && x.FechaHora >= desde && x.FechaHora < hasta)
            .GroupBy(x => x.TipoRegistroId)
            .Select(x => new { TipoId = x.Key, Cantidad = x.Sum(y => y.CantidadInicial), Frecuencia = x.Count() })
            .ToDictionaryAsync(x => x.TipoId, ct);
        var asignaciones = await db.Asignaciones.AsNoTracking()
            .Where(x => x.Estado == EstadoMovimiento.Vigente && x.FechaHora >= desde && x.FechaHora < hasta)
            .GroupBy(x => x.Entrada.TipoRegistroId)
            .Select(x => new { TipoId = x.Key, Cantidad = x.Sum(y => y.Cantidad), Frecuencia = x.Count() })
            .ToDictionaryAsync(x => x.TipoId, ct);
        var mermas = await db.Mermas.AsNoTracking()
            .Where(x => x.Estado == EstadoMovimiento.Vigente && x.FechaHora >= desde && x.FechaHora < hasta)
            .GroupBy(x => x.Entrada.TipoRegistroId)
            .Select(x => new { TipoId = x.Key, Cantidad = x.Sum(y => y.Cantidad), Frecuencia = x.Count() })
            .ToDictionaryAsync(x => x.TipoId, ct);

        var items = tipos.Select(tipo =>
        {
            entradas.TryGetValue(tipo.Id, out var entrada);
            asignaciones.TryGetValue(tipo.Id, out var asignacion);
            mermas.TryGetValue(tipo.Id, out var merma);
            var totalEntrada = entrada?.Cantidad ?? 0;
            var totalAsignacion = asignacion?.Cantidad ?? 0;
            var totalMerma = merma?.Cantidad ?? 0;
            return new InformeTipoItemDto(tipo.Id, tipo.Nombre, tipo.UnidadMedida,
                totalEntrada, entrada?.Frecuencia ?? 0, totalAsignacion, asignacion?.Frecuencia ?? 0,
                totalMerma, merma?.Frecuencia ?? 0, totalEntrada - totalAsignacion - totalMerma,
                totalEntrada == 0 ? 0 : totalMerma / totalEntrada * 100);
        }).ToList();
        return new(filtro, items, clock.UtcNow, current.UserName);
    }

    private static void ValidarRango(DateTime desde, DateTime hasta)
    {
        if (desde.Date > hasta.Date)
            throw new DomainException("La fecha desde no puede ser posterior a la fecha hasta.");
        if ((hasta.Date - desde.Date).TotalDays > 3660)
            throw new DomainException("El período consultado no puede superar diez años.");
    }
}

public sealed class AuditoriaConsultaService(ApplicationDbContext db) : IAuditoriaConsultaService
{
    public async Task<SistemaGestion.Application.Common.PagedResult<AuditoriaItemDto>> ListarAsync(
        AuditoriaFiltro filtro, CancellationToken ct)
    {
        if (filtro.FechaDesde.HasValue && filtro.FechaHasta.HasValue &&
            filtro.FechaDesde.Value.Date > filtro.FechaHasta.Value.Date)
            throw new DomainException("La fecha desde no puede ser posterior a la fecha hasta.");
        var query = db.Auditorias.AsNoTracking().AsQueryable();
        if (filtro.FechaDesde.HasValue) query = query.Where(x => x.FechaHora >= filtro.FechaDesde.Value.Date);
        if (filtro.FechaHasta.HasValue) query = query.Where(x => x.FechaHora < filtro.FechaHasta.Value.Date.AddDays(1));
        if (!string.IsNullOrWhiteSpace(filtro.Entidad)) query = query.Where(x => x.Entidad.Contains(filtro.Entidad));
        if (!string.IsNullOrWhiteSpace(filtro.Accion)) query = query.Where(x => x.Accion.Contains(filtro.Accion));
        if (!string.IsNullOrWhiteSpace(filtro.Usuario)) query = query.Where(x => x.NombreUsuario.Contains(filtro.Usuario));
        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(x => x.FechaHora).ThenByDescending(x => x.Id)
            .Skip((filtro.Pagina - 1) * filtro.TamanoPagina).Take(filtro.TamanoPagina)
            .Select(x => new AuditoriaItemDto(x.Id, x.FechaHora, x.NombreUsuario, x.Accion, x.Entidad,
                x.ClavePrimaria, x.CorrelationId, x.Motivo, x.DireccionIp))
            .ToListAsync(ct);
        return new(items, filtro.Pagina, filtro.TamanoPagina, total);
    }
}

public sealed class ExportacionService : IExportacionService
{
    private static readonly CultureInfo EsCl = CultureInfo.GetCultureInfo("es-CL");

    public ArchivoExportado IndicadoresCsv(IndicadorMermaDto indicador)
    {
        var filas = new List<string[]>
        {
            new[] { "Motivo", "Tipo", "Cantidad", "Unidad", "Frecuencia", "% mermas", "% entradas", "% acumulado" }
        };
        filas.AddRange(indicador.Items.Select(x => new[]
        {
            x.Motivo, x.Tipo, F(x.Cantidad), x.UnidadMedida, x.Frecuencia.ToString(EsCl),
            F(x.PorcentajeMermas), F(x.PorcentajeEntradas), F(x.PorcentajeAcumulado)
        }));
        filas.Add(new[] { "TOTAL", "", F(indicador.TotalMermas), "", "", "100", "", "" });
        return Csv(filas, $"indicadores-merma-{Sufijo(indicador.Filtro.FechaDesde, indicador.Filtro.FechaHasta)}.csv");
    }

    public ArchivoExportado IndicadoresPdf(IndicadorMermaDto indicador) =>
        Pdf("Indicadores de merma",
            $"Período: {Fecha(indicador.Filtro.FechaDesde)} al {Fecha(indicador.Filtro.FechaHasta)} · Entradas: {F(indicador.TotalEntradas)} · Mermas: {F(indicador.TotalMermas)}",
            new[] { "Motivo / tipo", "Cantidad", "Frecuencia", "% merma", "% acumulado" },
            indicador.Items.Select(x => new[] { $"{x.Motivo} / {x.Tipo}", $"{F(x.Cantidad)} {x.UnidadMedida}", x.Frecuencia.ToString(), $"{F(x.PorcentajeMermas)}%", $"{F(x.PorcentajeAcumulado)}%" }),
            $"indicadores-merma-{Sufijo(indicador.Filtro.FechaDesde, indicador.Filtro.FechaHasta)}.pdf");

    public ArchivoExportado InformeTrabajadorCsv(InformeTrabajadorDto informe)
    {
        var filas = new List<string[]>
        {
            new[] { "Trabajador", informe.Trabajador }, new[] { "RUT", informe.Rut }, new[] { "Área", informe.Area },
            new[] { "Período", $"{Fecha(informe.Filtro.FechaDesde)} al {Fecha(informe.Filtro.FechaHasta)}" }, Array.Empty<string>(),
            new[] { "Asignación", "Fecha", "Entrada", "Documento", "Tipo", "Cantidad", "Unidad", "Estado" }
        };
        filas.AddRange(informe.Items.Select(x => new[]
        {
            x.AsignacionId.ToString(), x.FechaHora.ToLocalTime().ToString("g", EsCl), x.EntradaId.ToString(),
            x.DocumentoOrigen, x.Tipo, F(x.Cantidad), x.UnidadMedida, x.Estado.ToString()
        }));
        filas.Add(new[] { "TOTAL VIGENTE", "", "", "", "", F(informe.Total), "", "" });
        return Csv(filas, $"informe-trabajador-{informe.TrabajadorId}-{Sufijo(informe.Filtro.FechaDesde, informe.Filtro.FechaHasta)}.csv");
    }

    public ArchivoExportado InformeTrabajadorPdf(InformeTrabajadorDto informe) =>
        Pdf($"Informe de asignaciones · {informe.Trabajador}",
            $"RUT: {informe.Rut} · Área: {informe.Area} · Período: {Fecha(informe.Filtro.FechaDesde)} al {Fecha(informe.Filtro.FechaHasta)} · Total vigente: {F(informe.Total)}",
            new[] { "Fecha", "Documento", "Tipo", "Cantidad", "Estado" },
            informe.Items.Select(x => new[] { x.FechaHora.ToLocalTime().ToString("g", EsCl), x.DocumentoOrigen, x.Tipo, $"{F(x.Cantidad)} {x.UnidadMedida}", x.Estado.ToString() }),
            $"informe-trabajador-{informe.TrabajadorId}-{Sufijo(informe.Filtro.FechaDesde, informe.Filtro.FechaHasta)}.pdf");

    public ArchivoExportado InformeTiposCsv(InformeTipoDto informe)
    {
        var filas = new List<string[]>
        {
            new[] { "Tipo", "Unidad", "Entradas", "N° entradas", "Asignaciones", "N° asignaciones", "Mermas", "N° mermas", "Saldo", "% merma" }
        };
        filas.AddRange(informe.Items.Select(x => new[]
        {
            x.Tipo, x.UnidadMedida, F(x.Entradas), x.CantidadEntradas.ToString(), F(x.Asignaciones),
            x.CantidadAsignaciones.ToString(), F(x.Mermas), x.CantidadMermas.ToString(), F(x.Saldo), F(x.PorcentajeMerma)
        }));
        return Csv(filas, $"informe-tipos-{Sufijo(informe.Filtro.FechaDesde, informe.Filtro.FechaHasta)}.csv");
    }

    public ArchivoExportado InformeTiposPdf(InformeTipoDto informe) =>
        Pdf("Informe consolidado por tipo",
            $"Período: {Fecha(informe.Filtro.FechaDesde)} al {Fecha(informe.Filtro.FechaHasta)}",
            new[] { "Tipo", "Entradas", "Asignaciones", "Mermas", "Saldo", "% merma" },
            informe.Items.Select(x => new[] { x.Tipo, F(x.Entradas), F(x.Asignaciones), F(x.Mermas), F(x.Saldo), $"{F(x.PorcentajeMerma)}%" }),
            $"informe-tipos-{Sufijo(informe.Filtro.FechaDesde, informe.Filtro.FechaHasta)}.pdf");

    private static ArchivoExportado Csv(IEnumerable<string[]> filas, string nombre)
    {
        var contenido = string.Join("\r\n", filas.Select(x => string.Join(';', x.Select(Escape))));
        return new(new UTF8Encoding(true).GetBytes(contenido), "text/csv; charset=utf-8", nombre);
    }

    private static ArchivoExportado Pdf(string titulo, string resumen, string[] encabezados, IEnumerable<string[]> filas, string nombre)
    {
        // El documento se genera en memoria para permitir una descarga directa sin archivos temporales.
        var datos = filas.ToList();
        var documento = Document.Create(container => container.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(28);
            page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Arial"));
            page.Header().Column(column =>
            {
                column.Item().Text("SG-OPERACIONES").FontSize(9).FontColor("#64748B").SemiBold();
                column.Item().Text(titulo).FontSize(20).FontColor("#0F172A").Bold();
                column.Item().PaddingTop(4).Text(resumen).FontColor("#475569");
                column.Item().PaddingTop(3).Text($"Generado: {DateTime.Now:dd-MM-yyyy HH:mm} · Zona horaria: America/Santiago").FontSize(7).FontColor("#64748B");
            });
            page.Content().PaddingVertical(16).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    for (var i = 0; i < encabezados.Length; i++) columns.RelativeColumn();
                });
                table.Header(header =>
                {
                    foreach (var texto in encabezados)
                        header.Cell().Background("#0F172A").Padding(6).Text(texto).FontColor(Colors.White).SemiBold();
                });
                foreach (var fila in datos)
                    foreach (var celda in fila)
                        table.Cell().BorderBottom(0.5f).BorderColor("#CBD5E1").Padding(6).Text(celda ?? "");
            });
            page.Footer().AlignCenter().Text(text =>
            {
                text.Span("Transacciones protegidas y auditadas · Página ");
                text.CurrentPageNumber();
                text.Span(" de ");
                text.TotalPages();
            });
        }));
        return new(documento.GeneratePdf(), "application/pdf", nombre);
    }

    private static string Escape(string? value)
    {
        value ??= "";
        return value.Contains(';') || value.Contains('"') || value.Contains('\n')
            ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
    }
    private static string F(decimal value) => value.ToString("0.###", EsCl);
    private static string Fecha(DateTime value) => value.ToString("dd-MM-yyyy", EsCl);
    private static string Sufijo(DateTime desde, DateTime hasta) => $"{desde:yyyyMMdd}-{hasta:yyyyMMdd}";
}
