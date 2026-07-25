using System.ComponentModel.DataAnnotations;

namespace SistemaGestion.Web.Models;

public sealed record UsuarioListadoViewModel(string Id, string Email, string NombreCompleto, bool Activo, string Roles);

public sealed class UsuarioCrearViewModel
{
    [Required, EmailAddress, Display(Name = "Correo electrónico")] public string Email { get; set; } = "";
    [Required, StringLength(150), Display(Name = "Nombre completo")] public string NombreCompleto { get; set; } = "";
    [Required, DataType(DataType.Password), MinLength(10), Display(Name = "Contraseña inicial")] public string Password { get; set; } = "";
    [Required, Display(Name = "Rol")] public string Rol { get; set; } = "";
    public IReadOnlyList<string> RolesDisponibles { get; set; } = [];
}
