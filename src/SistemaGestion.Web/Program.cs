using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using QuestPDF.Infrastructure;
using SistemaGestion.Application.Abstractions;
using SistemaGestion.Infrastructure;
using SistemaGestion.Infrastructure.Persistence;
using SistemaGestion.Web.Services;

// QuestPDF requiere declarar la licencia antes de registrar los servicios que generan informes.
QuestPDF.Settings.License = LicenseType.Community;
var builder = WebApplication.CreateBuilder(args);

// La composición se mantiene en el punto de entrada para que las dependencias y políticas sean auditables.
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddOptions<SystemOptions>()
    .Bind(builder.Configuration.GetSection(SystemOptions.SectionName))
    .ValidateDataAnnotations().ValidateOnStart();
var systemOptions = builder.Configuration.GetSection(SystemOptions.SectionName).Get<SystemOptions>() ?? new();
builder.Services.AddControllersWithViews(options =>
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute()));
builder.Services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>("sql-server", tags: ["ready"]);
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("login", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "local",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5, Window = TimeSpan.FromMinutes(1), QueueLimit = 0,
            AutoReplenishment = true
        }));
});
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Cuenta/IniciarSesion";
    options.AccessDeniedPath = "/Cuenta/AccesoDenegado";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(systemOptions.SessionMinutes);
    options.SlidingExpiration = true;
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdministrarCatalogos", p => p.RequireRole("Administrador"));
    options.AddPolicy("GestionarTrabajadores", p => p.RequireRole("Administrador"));
    options.AddPolicy("LecturaOperacional", p => p.RequireRole("Administrador", "Bodega", "Supervisor", "Consulta"));
    options.AddPolicy("CrearMovimientos", p => p.RequireRole("Administrador", "Bodega"));
    options.AddPolicy("GenerarReportes", p => p.RequireRole("Administrador", "Supervisor"));
    options.AddPolicy("ConsultarAuditoria", p => p.RequireRole("Administrador"));
    options.AddPolicy("AnularMovimientos", p => p.RequireRole("Administrador", "Supervisor"));
});

var app = builder.Build();
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Home/Error"); app.UseHsts(); }
app.UseStatusCodePagesWithReExecute("/Home/Estado", "?codigo={0}");
app.UseHttpsRedirection();

// Cada respuesta incorpora trazabilidad y encabezados defensivos sin depender del controlador ejecutado.
app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
        ?? context.TraceIdentifier;
    context.TraceIdentifier = correlationId;
    context.Response.Headers["X-Correlation-ID"] = correlationId;
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    context.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; font-src 'self' https://fonts.gstatic.com; script-src 'self' 'unsafe-inline'; img-src 'self' data:";
    using (app.Logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
        await next();
});
app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.Use(async (context, next) =>
{
    await next();
    if (context.Response.StatusCode is 401 or 403 && context.User.Identity?.IsAuthenticated == true)
    {
        var audit = context.RequestServices.GetRequiredService<IAuditoriaService>();
        await audit.RegistrarYGuardarAsync("AccesoDenegado", "Ruta", context.Request.Path,
            new { Method = context.Request.Method, StatusCode = context.Response.StatusCode },
            context.RequestAborted);
    }
});
app.UseAuthorization();

// Liveness confirma el proceso; readiness también valida la conexión con la base de datos.
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
if (app.Environment.IsDevelopment()) await SeedData.InitializeAsync(app.Services);
app.Run();

public partial class Program;
