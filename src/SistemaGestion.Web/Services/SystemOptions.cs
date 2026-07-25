using System.ComponentModel.DataAnnotations;

namespace SistemaGestion.Web.Services;

public sealed class SystemOptions
{
    public const string SectionName = "System";
    [Required] public string TimeZoneId { get; set; } = "Pacific SA Standard Time";
    [Range(5, 240)] public int SessionMinutes { get; set; } = 30;
}
