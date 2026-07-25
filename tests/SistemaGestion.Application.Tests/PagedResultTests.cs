using SistemaGestion.Application.Common;

namespace SistemaGestion.Application.Tests;

public sealed class PagedResultTests
{
    [Fact]
    public void Calcula_metadatos_de_paginacion()
    {
        var result = new PagedResult<int>([11, 12], 2, 10, 25);
        Assert.Equal(3, result.TotalPages);
        Assert.True(result.HasPreviousPage);
        Assert.True(result.HasNextPage);
    }
}
