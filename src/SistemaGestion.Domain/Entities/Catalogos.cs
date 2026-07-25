using SistemaGestion.Domain.Enums;
using SistemaGestion.Domain.Exceptions;

namespace SistemaGestion.Domain.Entities;

public sealed class Trabajador : EntidadBase
{
    private Trabajador() { }
    public Trabajador(string rut, string nombreCompleto, string area, string creadoPor)
    {
        Rut = Validar(rut, "El RUT es obligatorio.").ToUpperInvariant();
        NombreCompleto = Validar(nombreCompleto, "El nombre es obligatorio.");
        Area = Validar(area, "El área es obligatoria.");
        CreadoPor = Validar(creadoPor, "El usuario creador es obligatorio.");
    }
    public string Rut { get; private set; } = "";
    public string NombreCompleto { get; private set; } = "";
    public string Area { get; private set; } = "";
    public EstadoCatalogo Estado { get; private set; } = EstadoCatalogo.Activo;
    public string CreadoPor { get; private set; } = "";
    public string? ModificadoPor { get; private set; }
    public void Editar(string nombre, string area, string usuario)
    {
        NombreCompleto = Validar(nombre, "El nombre es obligatorio.");
        Area = Validar(area, "El área es obligatoria.");
        Modificar(usuario);
    }
    public void Activar(string usuario) { Estado = EstadoCatalogo.Activo; Modificar(usuario); }
    public void Desactivar(string usuario) { Estado = EstadoCatalogo.Inactivo; Modificar(usuario); }
    private void Modificar(string usuario) { ModificadoPor = Validar(usuario, "El usuario es obligatorio."); FechaModificacion = DateTime.UtcNow; }
    private static string Validar(string value, string message) => string.IsNullOrWhiteSpace(value) ? throw new DomainException(message) : value.Trim();
}

public sealed class TipoRegistro : EntidadBase
{
    private TipoRegistro() { }
    public TipoRegistro(string nombre, string unidadMedida)
    {
        Nombre = Requerido(nombre, "El nombre es obligatorio.");
        UnidadMedida = Requerido(unidadMedida, "La unidad de medida es obligatoria.");
    }
    public string Nombre { get; private set; } = "";
    public string UnidadMedida { get; private set; } = "";
    public EstadoCatalogo Estado { get; private set; } = EstadoCatalogo.Activo;
    public void Editar(string nombre, string unidad) { Nombre = Requerido(nombre, "El nombre es obligatorio."); UnidadMedida = Requerido(unidad, "La unidad es obligatoria."); FechaModificacion = DateTime.UtcNow; }
    public void Activar() { Estado = EstadoCatalogo.Activo; FechaModificacion = DateTime.UtcNow; }
    public void Desactivar() { Estado = EstadoCatalogo.Inactivo; FechaModificacion = DateTime.UtcNow; }
    private static string Requerido(string value, string message) => string.IsNullOrWhiteSpace(value) ? throw new DomainException(message) : value.Trim();
}

public sealed class MotivoMerma : EntidadBase
{
    private MotivoMerma() { }
    public MotivoMerma(string nombre, string? descripcion, bool requiereEvidencia, bool requiereAutorizacion)
    {
        Nombre = string.IsNullOrWhiteSpace(nombre) ? throw new DomainException("El nombre es obligatorio.") : nombre.Trim();
        Descripcion = descripcion?.Trim();
        RequiereEvidencia = requiereEvidencia;
        RequiereAutorizacion = requiereAutorizacion;
    }
    public string Nombre { get; private set; } = "";
    public string? Descripcion { get; private set; }
    public bool RequiereEvidencia { get; private set; }
    public bool RequiereAutorizacion { get; private set; }
    public EstadoCatalogo Estado { get; private set; } = EstadoCatalogo.Activo;
    public void Editar(string nombre, string? descripcion, bool evidencia, bool autorizacion)
    {
        Nombre = string.IsNullOrWhiteSpace(nombre) ? throw new DomainException("El nombre es obligatorio.") : nombre.Trim();
        Descripcion = descripcion?.Trim(); RequiereEvidencia = evidencia; RequiereAutorizacion = autorizacion; FechaModificacion = DateTime.UtcNow;
    }
    public void Activar() { Estado = EstadoCatalogo.Activo; FechaModificacion = DateTime.UtcNow; }
    public void Desactivar() { Estado = EstadoCatalogo.Inactivo; FechaModificacion = DateTime.UtcNow; }
}

