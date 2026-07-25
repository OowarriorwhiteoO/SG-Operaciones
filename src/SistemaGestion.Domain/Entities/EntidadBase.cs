namespace SistemaGestion.Domain.Entities;

public abstract class EntidadBase
{
    public int Id { get; protected set; }
    public DateTime FechaCreacion { get; protected set; } = DateTime.UtcNow;
    public DateTime? FechaModificacion { get; protected set; }
    public byte[] RowVersion { get; protected set; } = [];
}

