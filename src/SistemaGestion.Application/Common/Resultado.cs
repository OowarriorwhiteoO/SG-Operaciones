namespace SistemaGestion.Application.Common;

public sealed record Resultado(bool Exitoso, string? Error = null)
{
    public static Resultado Ok() => new(true);
    public static Resultado Fallo(string error) => new(false, error);
}

public sealed record Resultado<T>(bool Exitoso, T? Valor = default, string? Error = null)
{
    public static Resultado<T> Ok(T valor) => new(true, valor);
    public static Resultado<T> Fallo(string error) => new(false, default, error);
}

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items, int PageNumber, int PageSize, int TotalItems)
{
    public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

