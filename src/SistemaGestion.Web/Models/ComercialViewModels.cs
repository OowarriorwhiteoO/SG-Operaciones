using SistemaGestion.Application.DTOs;

namespace SistemaGestion.Web.Models;

/// <summary>
/// Reúne el formulario y los catálogos necesarios para crear una cotización.
/// </summary>
public sealed class CotizacionCrearViewModel
{
    public CotizacionInput Input { get; set; } = new();
    public IReadOnlyList<ClienteDto> Clientes { get; set; } = [];
    public IReadOnlyList<ProductoServicioDto> Productos { get; set; } = [];
}

