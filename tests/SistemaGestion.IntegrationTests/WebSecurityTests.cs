using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace SistemaGestion.IntegrationTests;

public sealed class WebSecurityTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    public WebSecurityTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Production"));

    [Fact]
    public async Task Health_live_responde_y_aplica_cabeceras_defensivas()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var response = await client.GetAsync("/health/live");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.True(response.Headers.Contains("Content-Security-Policy"));
        Assert.True(response.Headers.Contains("X-Correlation-ID"));
    }

    [Fact]
    public async Task Anonimo_es_redirigido_y_post_sin_antiforgery_es_rechazado()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var protectedResponse = await client.GetAsync("/");
        Assert.Equal(HttpStatusCode.Redirect, protectedResponse.StatusCode);
        Assert.Contains("/Cuenta/IniciarSesion", protectedResponse.Headers.Location?.OriginalString);
        var post = await client.PostAsync("/Cuenta/IniciarSesion",
            new FormUrlEncodedContent(new Dictionary<string, string> { ["Email"] = "invalid@example.local", ["Password"] = "invalid" }));
        Assert.Equal(HttpStatusCode.BadRequest, post.StatusCode);
    }
}
