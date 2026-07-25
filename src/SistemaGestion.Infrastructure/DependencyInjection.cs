using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SistemaGestion.Application.Abstractions;
using SistemaGestion.Infrastructure.Identity;
using SistemaGestion.Infrastructure.Persistence;
using SistemaGestion.Infrastructure.Services;

namespace SistemaGestion.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connection = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Falta ConnectionStrings:DefaultConnection.");
        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connection));
        services.AddIdentity<ApplicationUser, Microsoft.AspNetCore.Identity.IdentityRole>(options =>
        {
            options.Password.RequiredLength = 10;
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.User.RequireUniqueEmail = true;
        }).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
        services.AddScoped<ITrabajadorService, TrabajadorService>();
        services.AddScoped<ITipoRegistroService, TipoRegistroService>();
        services.AddScoped<IMotivoMermaService, MotivoMermaService>();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddScoped<IAuditoriaService, AuditoriaService>();
        services.AddScoped<ISaldoService, SaldoService>();
        services.AddScoped<IEntradaService, EntradaService>();
        services.AddScoped<IAsignacionService, AsignacionService>();
        services.AddScoped<IMermaService, MermaService>();
        services.AddScoped<IAnulacionService, AnulacionService>();
        services.AddScoped<IReporteService, ReporteService>();
        services.AddScoped<IExportacionService, ExportacionService>();
        services.AddScoped<IAuditoriaConsultaService, AuditoriaConsultaService>();
        return services;
    }
}
