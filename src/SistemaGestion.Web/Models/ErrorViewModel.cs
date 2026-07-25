namespace SistemaGestion.Web.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public string Message { get; set; } = "Ocurrió un error inesperado. Intente nuevamente.";

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
