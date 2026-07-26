using System.Globalization;

namespace SistemaGestion.Web.Helpers;

/// <summary>
/// Presenta importes con el símbolo, separadores y decimales propios de la moneda global.
/// No realiza conversión de valores ni consulta tipos de cambio.
/// </summary>
public static class FormatoMoneda
{
    private static readonly CultureInfo CulturaClp = CrearCultura("es-CL", "$", 0);
    private static readonly CultureInfo CulturaUsd = CrearCultura("en-US", "US$", 2);
    private static readonly CultureInfo CulturaEur = CrearCultura("es-ES", "€", 2);

    public static string ComoMoneda(this decimal valor, string? codigo) =>
        valor.ToString("C", codigo?.ToUpperInvariant() switch
        {
            "USD" => CulturaUsd,
            "EUR" => CulturaEur,
            _ => CulturaClp
        });

    private static CultureInfo CrearCultura(string nombre, string simbolo, int decimales)
    {
        var cultura = (CultureInfo)CultureInfo.GetCultureInfo(nombre).Clone();
        cultura.NumberFormat.CurrencySymbol = simbolo;
        cultura.NumberFormat.CurrencyDecimalDigits = decimales;
        cultura.NumberFormat.CurrencyPositivePattern = 0;
        cultura.NumberFormat.CurrencyNegativePattern = 1;
        return CultureInfo.ReadOnly(cultura);
    }
}
