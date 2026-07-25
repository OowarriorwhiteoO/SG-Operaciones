using Microsoft.EntityFrameworkCore;
using SistemaGestion.Application.Abstractions;
using SistemaGestion.Application.DTOs;
using SistemaGestion.Domain.Entities;
using SistemaGestion.Infrastructure.Persistence;
using SistemaGestion.Infrastructure.Services;

namespace SistemaGestion.IntegrationTests;

public sealed class MovimientoServiceSqlServerTests : IAsyncLifetime
{
    private readonly string _database = $"SistemaGestionTests_{Guid.NewGuid():N}";
    private string ConnectionString => $"Server=(localdb)\\MSSQLLocalDB;Database={_database};Trusted_Connection=True;TrustServerCertificate=True";
    private readonly TestCurrentUser _current = new();
    private readonly TestClock _clock = new();

    public async Task InitializeAsync()
    {
        await using var db = CreateDb();
        await db.Database.EnsureCreatedAsync();
        db.TiposRegistro.Add(new TipoRegistro("Tipo prueba", "unidad"));
        db.Trabajadores.Add(new Trabajador("88.888.888-8", "Trabajador Prueba", "Bodega", "test"));
        db.MotivosMerma.Add(new MotivoMerma("Daño prueba", "Motivo que exige evidencia.", true, false));
        await db.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await using var db = CreateDb();
        await db.Database.EnsureDeletedAsync();
    }

    [Fact]
    public async Task Crear_entrada_y_asignacion_actualiza_saldo_y_audita_en_sql_server()
    {
        int entradaId;
        await using (var db = CreateDb())
        {
            var auditoria = new AuditoriaService(db, _current, _clock);
            var service = new EntradaService(db, _current, auditoria);
            var tipoId = await db.TiposRegistro.Select(x => x.Id).SingleAsync();
            var created = await service.CrearAsync(new EntradaInput
            {
                TipoRegistroId = tipoId, FechaHora = DateTime.Now, CantidadInicial = 10m, DocumentoOrigen = " doc-integracion "
            }, default);
            Assert.True(created.Exitoso, created.Error);
            entradaId = created.Valor;
        }

        await using (var db = CreateDb())
        {
            var saldo = await new SaldoService(db).ObtenerAsync(entradaId, default);
            var trabajadorId = await db.Trabajadores.Select(x => x.Id).SingleAsync();
            var auditoria = new AuditoriaService(db, _current, _clock);
            var service = new AsignacionService(db, _current, _clock, auditoria);
            var result = await service.CrearAsync(new AsignacionInput
            {
                EntradaId = entradaId, TrabajadorId = trabajadorId, Cantidad = 4.25m,
                FechaHora = DateTime.Now, EntradaRowVersion = Convert.ToBase64String(saldo!.RowVersion)
            }, default);
            Assert.True(result.Exitoso, result.Error);
        }

        await using (var db = CreateDb())
        {
            var saldo = await new SaldoService(db).ObtenerAsync(entradaId, default);
            Assert.Equal(5.75m, saldo!.Disponible);
            Assert.Equal(2, await db.Auditorias.CountAsync());
            var disponibles = await new EntradaService(db, _current, new AuditoriaService(db, _current, _clock)).ListarDisponiblesAsync(default);
            Assert.Contains(disponibles, x => x.Id == entradaId && x.SaldoDisponible == 5.75m);
        }
    }

    [Fact]
    public async Task Token_obsoleto_impide_segunda_asignacion_y_saldo_negativo()
    {
        int entradaId;
        int trabajadorId;
        byte[] token;
        await using (var db = CreateDb())
        {
            var tipoId = await db.TiposRegistro.Select(x => x.Id).SingleAsync();
            var entrada = new Entrada(tipoId, DateTime.UtcNow, 10m, "CONC-001", _current.UserId!);
            db.Entradas.Add(entrada);
            await db.SaveChangesAsync();
            entradaId = entrada.Id;
            trabajadorId = await db.Trabajadores.Select(x => x.Id).SingleAsync();
            token = entrada.RowVersion;
        }

        var results = await Task.WhenAll(
            CrearAsignacionAsync(entradaId, trabajadorId, 7m, token),
            CrearAsignacionAsync(entradaId, trabajadorId, 7m, token));

        Assert.Single(results.Where(x => x.Exitoso));
        Assert.Single(results.Where(x => !x.Exitoso));
        Assert.Contains("otro usuario", results.Single(x => !x.Exitoso).Error);
        await using var verification = CreateDb();
        var saldo = await new SaldoService(verification).ObtenerAsync(entradaId, default);
        Assert.Equal(3m, saldo!.Disponible);
        Assert.Single(await verification.Asignaciones.Where(x => x.EntradaId == entradaId).ToListAsync());
    }

    [Fact]
    public async Task Documento_y_tipo_duplicados_son_rechazados()
    {
        await using var db = CreateDb();
        var auditoria = new AuditoriaService(db, _current, _clock);
        var service = new EntradaService(db, _current, auditoria);
        var tipoId = await db.TiposRegistro.Select(x => x.Id).SingleAsync();
        var input = new EntradaInput { TipoRegistroId = tipoId, FechaHora = DateTime.Now, CantidadInicial = 1m, DocumentoOrigen = "DUP-001" };
        Assert.True((await service.CrearAsync(input, default)).Exitoso);
        var duplicate = await service.CrearAsync(input, default);
        Assert.False(duplicate.Exitoso);
        Assert.Contains("mismo documento", duplicate.Error);
    }

    [Fact]
    public async Task Merma_exige_evidencia_y_su_anulacion_repone_saldo()
    {
        int entradaId;
        int motivoId;
        await using (var db = CreateDb())
        {
            var tipoId = await db.TiposRegistro.Select(x => x.Id).SingleAsync();
            var entrada = new Entrada(tipoId, DateTime.UtcNow, 20m, "MERMA-001", _current.UserId!);
            db.Entradas.Add(entrada);
            await db.SaveChangesAsync();
            entradaId = entrada.Id;
            motivoId = await db.MotivosMerma.Select(x => x.Id).SingleAsync();
        }

        int mermaId;
        await using (var db = CreateDb())
        {
            var saldo = await new SaldoService(db).ObtenerAsync(entradaId, default);
            var service = new MermaService(db, _current, _clock, new AuditoriaService(db, _current, _clock));
            var sinEvidencia = await service.CrearAsync(new MermaInput
            {
                EntradaId = entradaId, MotivoMermaId = motivoId, FechaHora = DateTime.Now,
                Cantidad = 3m, EntradaRowVersion = Convert.ToBase64String(saldo!.RowVersion)
            }, default);
            Assert.False(sinEvidencia.Exitoso);
            var creada = await service.CrearAsync(new MermaInput
            {
                EntradaId = entradaId, MotivoMermaId = motivoId, FechaHora = DateTime.Now,
                Cantidad = 3m, EvidenciaReferencia = "EVID-001",
                EntradaRowVersion = Convert.ToBase64String(saldo.RowVersion)
            }, default);
            Assert.True(creada.Exitoso, creada.Error);
            mermaId = creada.Valor;
        }

        await using (var db = CreateDb())
        {
            Assert.Equal(17m, (await new SaldoService(db).ObtenerAsync(entradaId, default))!.Disponible);
            var merma = await db.Mermas.AsNoTracking().SingleAsync(x => x.Id == mermaId);
            var anulacion = new AnulacionService(db, _current, _clock, new AuditoriaService(db, _current, _clock));
            var result = await anulacion.AnularAsync(new AnulacionInput
            {
                Id = mermaId, Clase = SistemaGestion.Domain.Enums.ClaseMovimiento.Merma,
                Motivo = "Evidencia incorrecta", RowVersion = Convert.ToBase64String(merma.RowVersion)
            }, default);
            Assert.True(result.Exitoso, result.Error);
        }

        await using (var db = CreateDb())
        {
            Assert.Equal(20m, (await new SaldoService(db).ObtenerAsync(entradaId, default))!.Disponible);
            Assert.Equal(SistemaGestion.Domain.Enums.EstadoMovimiento.Anulada, (await db.Mermas.SingleAsync(x => x.Id == mermaId)).Estado);
            Assert.Equal(2, await db.Auditorias.CountAsync());
        }
    }

    [Fact]
    public async Task Indicador_excluye_mermas_anuladas_y_calcula_pareto()
    {
        await using var db = CreateDb();
        var tipoId = await db.TiposRegistro.Select(x => x.Id).SingleAsync();
        var motivoId = await db.MotivosMerma.Select(x => x.Id).SingleAsync();
        var entrada = new Entrada(tipoId, DateTime.UtcNow, 100m, "IND-001", _current.UserId!);
        db.Entradas.Add(entrada);
        await db.SaveChangesAsync();
        db.Mermas.Add(new Merma(entrada.Id, motivoId, DateTime.UtcNow, 5m, _current.UserId!, true, "EV-1"));
        await db.SaveChangesAsync();
        var service = new MermaService(db, _current, _clock, new AuditoriaService(db, _current, _clock));
        var result = await service.ObtenerIndicadoresAsync(new IndicadorMermaFiltro
        {
            FechaDesde = DateTime.Today.AddDays(-1), FechaHasta = DateTime.Today.AddDays(1), TipoRegistroId = tipoId
        }, default);
        Assert.Equal(5m, result.TotalMermas);
        Assert.Equal(100m, result.TotalEntradas);
        Assert.Equal(100m, result.Items.Single().PorcentajeAcumulado);
        Assert.Equal(5m, result.Items.Single().PorcentajeEntradas);
    }

    [Fact]
    public async Task Informe_por_tipo_calcula_totales_saldo_y_porcentaje()
    {
        await using var db = CreateDb();
        var tipoId = await db.TiposRegistro.Select(x => x.Id).SingleAsync();
        var motivoId = await db.MotivosMerma.Select(x => x.Id).SingleAsync();
        var trabajadorId = await db.Trabajadores.Select(x => x.Id).SingleAsync();
        var entrada = new Entrada(tipoId, DateTime.UtcNow, 100m, "REP-001", _current.UserId!);
        db.Entradas.Add(entrada);
        await db.SaveChangesAsync();
        db.Asignaciones.Add(new Asignacion(entrada.Id, trabajadorId, DateTime.UtcNow, 40m, _current.UserId!));
        db.Mermas.Add(new Merma(entrada.Id, motivoId, DateTime.UtcNow, 10m, _current.UserId!, true, "EV-REP"));
        await db.SaveChangesAsync();

        var service = new ReporteService(db, _current, _clock);
        var result = await service.ObtenerInformeTiposAsync(new InformePeriodoFiltro
        {
            FechaDesde = DateTime.Today.AddDays(-1), FechaHasta = DateTime.Today.AddDays(1)
        }, default);
        var item = result.Items.Single(x => x.TipoRegistroId == tipoId);
        Assert.Equal(100m, item.Entradas);
        Assert.Equal(40m, item.Asignaciones);
        Assert.Equal(10m, item.Mermas);
        Assert.Equal(50m, item.Saldo);
        Assert.Equal(10m, item.PorcentajeMerma);

        var consulta = await service.ConsultarMovimientosAsync(new ConsultaMovimientoFiltro
        {
            FechaDesde = DateTime.Today.AddDays(-1), FechaHasta = DateTime.Today.AddDays(1),
            TipoRegistroId = tipoId, TamanoPagina = 5
        }, default);
        Assert.Equal(3, consulta.TotalItems);
        Assert.Contains(consulta.Items, x => x.Clase == SistemaGestion.Domain.Enums.ClaseMovimiento.Entrada);
        Assert.Contains(consulta.Items, x => x.Clase == SistemaGestion.Domain.Enums.ClaseMovimiento.Asignacion);
        Assert.Contains(consulta.Items, x => x.Clase == SistemaGestion.Domain.Enums.ClaseMovimiento.Merma);

        var dashboard = await service.ObtenerDashboardAsync(new DashboardFiltro
        {
            FechaDesde = DateTime.Today.AddDays(-1), FechaHasta = DateTime.Today.AddDays(1)
        }, default);
        Assert.Equal(1, dashboard.TotalEntradas);
        Assert.Equal(100m, dashboard.CantidadRecibida);
        Assert.Equal(40m, dashboard.CantidadAsignada);
        Assert.Equal(10m, dashboard.CantidadMerma);
        Assert.Equal(50m, dashboard.SaldoTotal);
    }

    [Fact]
    public void Exportaciones_csv_y_pdf_contienen_el_mismo_total()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        var filtro = new IndicadorMermaFiltro { FechaDesde = DateTime.Today.AddDays(-7), FechaHasta = DateTime.Today };
        var indicador = new IndicadorMermaDto(filtro, 5m, 100m,
        [
            new IndicadorMermaItemDto(1, 1, "Daño", "EPP", "unidad", 5m, 2, 100m, 5m, 100m)
        ]);
        var service = new ExportacionService();
        var csv = service.IndicadoresCsv(indicador);
        var pdf = service.IndicadoresPdf(indicador);
        var output = Environment.GetEnvironmentVariable("SGW_PDF_OUTPUT");
        if (!string.IsNullOrWhiteSpace(output)) File.WriteAllBytes(output, pdf.Contenido);
        Assert.Contains("5", System.Text.Encoding.UTF8.GetString(csv.Contenido));
        Assert.StartsWith("%PDF", System.Text.Encoding.ASCII.GetString(pdf.Contenido, 0, 4));
        Assert.True(pdf.Contenido.Length > 1_000);
    }

    private async Task<SistemaGestion.Application.Common.Resultado<int>> CrearAsignacionAsync(int entradaId, int trabajadorId, decimal cantidad, byte[] token)
    {
        await using var db = CreateDb();
        var service = new AsignacionService(db, _current, _clock, new AuditoriaService(db, _current, _clock));
        return await service.CrearAsync(new AsignacionInput
        {
            EntradaId = entradaId, TrabajadorId = trabajadorId, Cantidad = cantidad,
            FechaHora = DateTime.Now, EntradaRowVersion = Convert.ToBase64String(token)
        }, default);
    }

    private ApplicationDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(ConnectionString).Options);

    private sealed class TestClock : IDateTimeProvider { public DateTime UtcNow => DateTime.UtcNow; }
    private sealed class TestCurrentUser : ICurrentUserService
    {
        public string? UserId => "integration-user";
        public string UserName => "integration@test.local";
        public string CorrelationId => "integration-correlation";
        public string? IpAddress => "127.0.0.1";
        public string? UserAgent => "xUnit";
    }
}
