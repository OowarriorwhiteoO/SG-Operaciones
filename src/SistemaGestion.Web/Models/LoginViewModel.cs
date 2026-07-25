using System.ComponentModel.DataAnnotations;

namespace SistemaGestion.Web.Models;

public sealed class LoginViewModel
{
    [Required, EmailAddress, Display(Name = "Correo electrónico")] public string Email { get; set; } = "";
    [Required, DataType(DataType.Password), Display(Name = "Contraseña")] public string Password { get; set; } = "";
    [Display(Name = "Recordarme")] public bool Recordarme { get; set; }
}
