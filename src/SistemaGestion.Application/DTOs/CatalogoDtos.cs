using System.ComponentModel.DataAnnotations;
using SistemaGestion.Domain.Enums;

namespace SistemaGestion.Application.DTOs;

public sealed record TrabajadorDto(int Id, string Rut, string NombreCompleto, string Area, EstadoCatalogo Estado, byte[] RowVersion);
public sealed record TipoRegistroDto(int Id, string Nombre, string UnidadMedida, EstadoCatalogo Estado, byte[] RowVersion);
public sealed record MotivoMermaDto(int Id, string Nombre, string? Descripcion, bool RequiereEvidencia, bool RequiereAutorizacion, EstadoCatalogo Estado, byte[] RowVersion);

public sealed class TrabajadorInput
{
    public int Id { get; set; }
    [Required, StringLength(20)] public string Rut { get; set; } = "";
    [Required, StringLength(150)] public string NombreCompleto { get; set; } = "";
    [Required, StringLength(100)] public string Area { get; set; } = "";
}

public sealed class TipoRegistroInput
{
    public int Id { get; set; }
    [Required, StringLength(100)] public string Nombre { get; set; } = "";
    [Required, StringLength(30)] public string UnidadMedida { get; set; } = "";
}

public sealed class MotivoMermaInput
{
    public int Id { get; set; }
    [Required, StringLength(100)] public string Nombre { get; set; } = "";
    [StringLength(500)] public string? Descripcion { get; set; }
    public bool RequiereEvidencia { get; set; }
    public bool RequiereAutorizacion { get; set; }
}

