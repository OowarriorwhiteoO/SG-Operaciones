using SistemaGestion.Web.Helpers;

namespace SistemaGestion.IntegrationTests;

public sealed class FormatoMonedaTests
{
    [Theory]
    [InlineData(10000, "CLP", "$10.000")]
    [InlineData(10000, null, "$10.000")]
    [InlineData(10000, "USD", "US$10,000.00")]
    [InlineData(10000, "EUR", "€10.000,00")]
    public void Formato_respeta_la_moneda_global(decimal valor, string? moneda, string esperado)
    {
        Assert.Equal(esperado, valor.ComoMoneda(moneda));
    }
}
