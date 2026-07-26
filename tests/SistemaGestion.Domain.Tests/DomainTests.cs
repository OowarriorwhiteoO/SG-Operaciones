using SistemaGestion.Domain.Entities;
using SistemaGestion.Domain.Enums;
using SistemaGestion.Domain.Exceptions;

namespace SistemaGestion.Domain.Tests;

public sealed class DomainTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Entrada_rechaza_cantidad_no_positiva(decimal cantidad)
    {
        var ex = Assert.Throws<DomainException>(() => new Entrada(1, DateTime.UtcNow, cantidad, "DOC-1", "usuario"));
        Assert.Contains("mayor que cero", ex.Message);
    }

    [Fact]
    public void Entrada_normaliza_documento_y_calcula_saldo_inicial()
    {
        var entrada = new Entrada(1, DateTime.UtcNow, 12.345m, "  oc-123  ", "usuario");
        Assert.Equal("OC-123", entrada.DocumentoOrigen);
        Assert.Equal(12.345m, entrada.CalcularSaldo());
    }

    [Fact]
    public void Trabajador_se_desactiva_sin_eliminarse()
    {
        var trabajador = new Trabajador("11.111.111-1", "Persona Demo", "Bodega", "admin");
        trabajador.Desactivar("admin");
        Assert.Equal(EstadoCatalogo.Inactivo, trabajador.Estado);
        Assert.NotNull(trabajador.FechaModificacion);
    }

    [Fact]
    public void Motivo_requiere_nombre()
    {
        Assert.Throws<DomainException>(() => new MotivoMerma("", null, false, false));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.001)]
    public void Asignacion_rechaza_cantidad_no_positiva(decimal cantidad)
    {
        Assert.Throws<DomainException>(() => new Asignacion(1, 1, DateTime.UtcNow, cantidad, "usuario"));
    }

    [Fact]
    public void Registrar_movimiento_actualiza_fecha_de_entrada()
    {
        var entrada = new Entrada(1, DateTime.UtcNow, 10m, "DOC-2", "usuario");
        var fecha = DateTime.UtcNow.AddMinutes(1);
        entrada.RegistrarMovimiento(fecha);
        Assert.Equal(fecha, entrada.FechaUltimoMovimiento);
        Assert.Equal(fecha, entrada.FechaModificacion);
    }

    [Fact]
    public void Merma_exige_evidencia_cuando_el_motivo_lo_indica()
    {
        var ex = Assert.Throws<DomainException>(() =>
            new Merma(1, 1, DateTime.UtcNow, 1m, "usuario", requiereEvidencia: true));
        Assert.Contains("evidencia", ex.Message);
    }

    [Fact]
    public void Anulacion_de_asignacion_es_logica_y_no_puede_repetirse()
    {
        var asignacion = new Asignacion(1, 1, DateTime.UtcNow, 2m, "usuario");
        asignacion.Anular("supervisor", "Registro duplicado", DateTime.UtcNow);
        Assert.Equal(EstadoMovimiento.Anulada, asignacion.Estado);
        Assert.Throws<DomainException>(() => asignacion.Anular("supervisor", "Otra vez", DateTime.UtcNow));
    }

    [Fact]
    public void Entrada_no_se_anula_con_movimientos_vigentes()
    {
        var entrada = new Entrada(1, DateTime.UtcNow, 10m, "DOC-3", "usuario");
        Assert.Throws<DomainException>(() =>
            entrada.Anular("supervisor", "Corrección", DateTime.UtcNow, tieneMovimientosVigentes: true));
        Assert.Equal(EstadoMovimiento.Vigente, entrada.Estado);
    }

    [Fact]
    public void Cotizacion_calcula_totales_y_controla_su_flujo()
    {
        var cotizacion = new Cotizacion("COT-TEST-001", 1, DateTime.Today, DateTime.Today.AddDays(15), "usuario", null);
        var linea = new CotizacionDetalle(1, "Servicio de prueba", 2m, 10000m, 10m, true, 19m);
        cotizacion.Detalles.Add(linea);
        cotizacion.EstablecerTotales(linea.TotalNeto, linea.MontoIva);

        Assert.Equal(18000m, cotizacion.SubtotalNeto);
        Assert.Equal(3420m, cotizacion.MontoIva);
        Assert.Equal(21420m, cotizacion.Total);

        cotizacion.Enviar();
        cotizacion.Aceptar();
        cotizacion.MarcarFacturada();
        Assert.Equal(EstadoCotizacion.Facturada, cotizacion.Estado);
        Assert.Throws<DomainException>(() => cotizacion.Rechazar());
    }

    [Fact]
    public void Factura_emitida_puede_registrar_un_solo_pago()
    {
        var factura = new Factura("FAC-TEST-001", 1, 1, DateTime.Today, DateTime.Today.AddDays(30),
            10000m, 1900m, "usuario");

        factura.MarcarPagada(DateTime.Today, "Transferencia 123");

        Assert.Equal(EstadoFactura.Pagada, factura.Estado);
        Assert.Equal("Transferencia 123", factura.ReferenciaPago);
        Assert.Throws<DomainException>(() => factura.MarcarPagada(DateTime.Today, "Otro pago"));
    }
}
