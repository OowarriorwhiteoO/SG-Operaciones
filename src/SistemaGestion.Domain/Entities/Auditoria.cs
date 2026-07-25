namespace SistemaGestion.Domain.Entities;

public sealed class Auditoria
{
    private Auditoria() { }
    public Auditoria(
        string? usuarioId,
        string nombreUsuario,
        string accion,
        string entidad,
        string clavePrimaria,
        DateTime fechaHora,
        string correlationId,
        string? valoresAnteriores = null,
        string? valoresNuevos = null,
        string? motivo = null,
        string? direccionIp = null,
        string? userAgent = null)
    {
        UsuarioId = usuarioId;
        NombreUsuario = nombreUsuario;
        Accion = accion;
        Entidad = entidad;
        ClavePrimaria = clavePrimaria;
        FechaHora = fechaHora;
        CorrelationId = correlationId;
        ValoresAnteriores = valoresAnteriores;
        ValoresNuevos = valoresNuevos;
        Motivo = motivo;
        DireccionIp = direccionIp;
        UserAgent = userAgent;
    }
    public long Id { get; private set; }
    public string? UsuarioId { get; private set; }
    public string NombreUsuario { get; private set; } = "";
    public string Accion { get; private set; } = "";
    public string Entidad { get; private set; } = "";
    public string ClavePrimaria { get; private set; } = "";
    public DateTime FechaHora { get; private set; }
    public string? ValoresAnteriores { get; private set; }
    public string? ValoresNuevos { get; private set; }
    public string? Motivo { get; private set; }
    public string? DireccionIp { get; private set; }
    public string? UserAgent { get; private set; }
    public string CorrelationId { get; private set; } = "";
}
