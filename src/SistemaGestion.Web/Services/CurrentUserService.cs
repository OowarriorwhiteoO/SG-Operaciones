using System.Security.Claims;
using SistemaGestion.Application.Abstractions;

namespace SistemaGestion.Web.Services;

public sealed class CurrentUserService(IHttpContextAccessor accessor) : ICurrentUserService
{
    public string? UserId => accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
    public string UserName => accessor.HttpContext?.User.Identity?.Name ?? "sistema";
    public string CorrelationId => accessor.HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString("N");
    public string? IpAddress => accessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
    public string? UserAgent => accessor.HttpContext?.Request.Headers.UserAgent.ToString();
}
