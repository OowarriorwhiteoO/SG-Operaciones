using Microsoft.AspNetCore.Identity;

namespace SistemaGestion.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser
{
    public string NombreCompleto { get; set; } = "";
    public bool Activo { get; set; } = true;
}

