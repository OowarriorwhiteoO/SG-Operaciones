using Microsoft.EntityFrameworkCore;
using SistemaGestion.Infrastructure.Persistence;

namespace SistemaGestion.IntegrationTests;

public sealed class ModelConfigurationTests
{
    [Fact]
    public void Modelo_contiene_indices_unicos_y_precision_decimal()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite("Data Source=:memory:").Options;
        using var db = new ApplicationDbContext(options);
        var entrada = db.Model.FindEntityType("SistemaGestion.Domain.Entities.Entrada")!;
        var unique = entrada.GetIndexes().Single(x => x.IsUnique);
        Assert.Equal(["DocumentoOrigen", "TipoRegistroId"], unique.Properties.Select(x => x.Name));
        Assert.Equal(18, entrada.FindProperty("CantidadInicial")!.GetPrecision());
        Assert.Equal(3, entrada.FindProperty("CantidadInicial")!.GetScale());
    }

    [Fact]
    public void Modelo_configura_rowversion_como_token_de_concurrencia()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite("Data Source=:memory:").Options;
        using var db = new ApplicationDbContext(options);
        var trabajador = db.Model.FindEntityType("SistemaGestion.Domain.Entities.Trabajador")!;
        Assert.True(trabajador.FindProperty("RowVersion")!.IsConcurrencyToken);
    }
}
