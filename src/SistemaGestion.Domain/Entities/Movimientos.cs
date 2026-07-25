using SistemaGestion.Domain.Enums;
using SistemaGestion.Domain.Exceptions;

namespace SistemaGestion.Domain.Entities;

public sealed class Entrada : EntidadBase
{
    private Entrada() { }
    public Entrada(int tipoRegistroId, DateTime fechaHora, decimal cantidadInicial, string documentoOrigen, string usuarioId, string? observacion = null)
    {
        if (cantidadInicial <= 0) throw new DomainException("La cantidad inicial debe ser mayor que cero.");
        if (string.IsNullOrWhiteSpace(documentoOrigen)) throw new DomainException("El documento de origen es obligatorio.");
        TipoRegistroId = tipoRegistroId > 0 ? tipoRegistroId : throw new DomainException("El tipo de registro es obligatorio.");
        FechaHora = fechaHora;
        CantidadInicial = cantidadInicial;
        DocumentoOrigen = NormalizarDocumento(documentoOrigen);
        UsuarioResponsableId = usuarioId;
        Observacion = observacion?.Trim();
        FechaUltimoMovimiento = DateTime.UtcNow;
    }
    public int TipoRegistroId { get; private set; }
    public TipoRegistro TipoRegistro { get; private set; } = null!;
    public DateTime FechaHora { get; private set; }
    public decimal CantidadInicial { get; private set; }
    public string DocumentoOrigen { get; private set; } = "";
    public string? Observacion { get; private set; }
    public EstadoMovimiento Estado { get; private set; } = EstadoMovimiento.Vigente;
    public string UsuarioResponsableId { get; private set; } = "";
    public DateTime FechaUltimoMovimiento { get; private set; }
    public string? AnuladaPorId { get; private set; }
    public DateTime? FechaAnulacion { get; private set; }
    public string? MotivoAnulacion { get; private set; }
    public ICollection<Asignacion> Asignaciones { get; private set; } = [];
    public ICollection<Merma> Mermas { get; private set; } = [];
    public decimal CalcularSaldo() => CantidadInicial
        - Asignaciones.Where(x => x.Estado == EstadoMovimiento.Vigente).Sum(x => x.Cantidad)
        - Mermas.Where(x => x.Estado == EstadoMovimiento.Vigente).Sum(x => x.Cantidad);
    public void RegistrarMovimiento(DateTime fechaUtc)
    {
        if (Estado != EstadoMovimiento.Vigente)
            throw new DomainException("La entrada está anulada.");
        FechaUltimoMovimiento = fechaUtc;
        FechaModificacion = fechaUtc;
    }
    public void Anular(string usuarioId, string motivo, DateTime fechaUtc, bool tieneMovimientosVigentes)
    {
        if (Estado == EstadoMovimiento.Anulada) throw new DomainException("La entrada ya está anulada.");
        if (tieneMovimientosVigentes) throw new DomainException("Debe anular previamente todas las asignaciones y mermas vigentes.");
        ValidarAnulacion(usuarioId, motivo);
        Estado = EstadoMovimiento.Anulada;
        AnuladaPorId = usuarioId;
        MotivoAnulacion = motivo.Trim();
        FechaAnulacion = fechaUtc;
        FechaModificacion = fechaUtc;
    }
    public static string NormalizarDocumento(string value) => value.Trim().ToUpperInvariant();
    private static void ValidarAnulacion(string usuario, string motivo)
    {
        if (string.IsNullOrWhiteSpace(usuario)) throw new DomainException("El usuario que anula es obligatorio.");
        if (string.IsNullOrWhiteSpace(motivo)) throw new DomainException("El motivo de anulación es obligatorio.");
    }
}

public sealed class Asignacion : EntidadBase
{
    private Asignacion() { }
    public Asignacion(int entradaId, int trabajadorId, DateTime fechaHora, decimal cantidad, string usuarioId, string? observacion = null)
    {
        if (entradaId <= 0) throw new DomainException("La entrada es obligatoria.");
        if (trabajadorId <= 0) throw new DomainException("El trabajador es obligatorio.");
        if (cantidad <= 0) throw new DomainException("La cantidad debe ser mayor que cero.");
        if (string.IsNullOrWhiteSpace(usuarioId)) throw new DomainException("El usuario responsable es obligatorio.");
        EntradaId = entradaId;
        TrabajadorId = trabajadorId;
        FechaHora = fechaHora;
        Cantidad = cantidad;
        UsuarioResponsableId = usuarioId;
        Observacion = observacion?.Trim();
    }
    public int EntradaId { get; private set; }
    public Entrada Entrada { get; private set; } = null!;
    public int TrabajadorId { get; private set; }
    public Trabajador Trabajador { get; private set; } = null!;
    public DateTime FechaHora { get; private set; }
    public decimal Cantidad { get; private set; }
    public string? Observacion { get; private set; }
    public EstadoMovimiento Estado { get; private set; } = EstadoMovimiento.Vigente;
    public string UsuarioResponsableId { get; private set; } = "";
    public string? AnuladaPorId { get; private set; }
    public DateTime? FechaAnulacion { get; private set; }
    public string? MotivoAnulacion { get; private set; }
    public void Anular(string usuarioId, string motivo, DateTime fechaUtc)
    {
        if (Estado == EstadoMovimiento.Anulada) throw new DomainException("La asignación ya está anulada.");
        if (string.IsNullOrWhiteSpace(usuarioId)) throw new DomainException("El usuario que anula es obligatorio.");
        if (string.IsNullOrWhiteSpace(motivo)) throw new DomainException("El motivo de anulación es obligatorio.");
        Estado = EstadoMovimiento.Anulada;
        AnuladaPorId = usuarioId;
        MotivoAnulacion = motivo.Trim();
        FechaAnulacion = fechaUtc;
        FechaModificacion = fechaUtc;
    }
}

public sealed class Merma : EntidadBase
{
    private Merma() { }
    public Merma(int entradaId, int motivoMermaId, DateTime fechaHora, decimal cantidad, string usuarioId,
        bool requiereEvidencia, string? evidenciaReferencia = null, string? observacion = null)
    {
        if (entradaId <= 0) throw new DomainException("La entrada es obligatoria.");
        if (motivoMermaId <= 0) throw new DomainException("El motivo de merma es obligatorio.");
        if (cantidad <= 0) throw new DomainException("La cantidad debe ser mayor que cero.");
        if (string.IsNullOrWhiteSpace(usuarioId)) throw new DomainException("El usuario responsable es obligatorio.");
        if (requiereEvidencia && string.IsNullOrWhiteSpace(evidenciaReferencia))
            throw new DomainException("El motivo seleccionado requiere una referencia de evidencia.");
        EntradaId = entradaId;
        MotivoMermaId = motivoMermaId;
        FechaHora = fechaHora;
        Cantidad = cantidad;
        UsuarioResponsableId = usuarioId;
        EvidenciaReferencia = evidenciaReferencia?.Trim();
        Observacion = observacion?.Trim();
    }
    public int EntradaId { get; private set; }
    public Entrada Entrada { get; private set; } = null!;
    public int MotivoMermaId { get; private set; }
    public MotivoMerma MotivoMerma { get; private set; } = null!;
    public DateTime FechaHora { get; private set; }
    public decimal Cantidad { get; private set; }
    public string? Observacion { get; private set; }
    public string? EvidenciaReferencia { get; private set; }
    public EstadoMovimiento Estado { get; private set; } = EstadoMovimiento.Vigente;
    public string UsuarioResponsableId { get; private set; } = "";
    public string? AnuladaPorId { get; private set; }
    public DateTime? FechaAnulacion { get; private set; }
    public string? MotivoAnulacion { get; private set; }
    public void Anular(string usuarioId, string motivo, DateTime fechaUtc)
    {
        if (Estado == EstadoMovimiento.Anulada) throw new DomainException("La merma ya está anulada.");
        if (string.IsNullOrWhiteSpace(usuarioId)) throw new DomainException("El usuario que anula es obligatorio.");
        if (string.IsNullOrWhiteSpace(motivo)) throw new DomainException("El motivo de anulación es obligatorio.");
        Estado = EstadoMovimiento.Anulada;
        AnuladaPorId = usuarioId;
        MotivoAnulacion = motivo.Trim();
        FechaAnulacion = fechaUtc;
        FechaModificacion = fechaUtc;
    }
}
