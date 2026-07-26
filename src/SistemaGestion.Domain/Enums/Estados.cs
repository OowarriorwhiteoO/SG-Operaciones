namespace SistemaGestion.Domain.Enums;

public enum EstadoCatalogo { Activo = 1, Inactivo = 2 }
public enum EstadoMovimiento { Vigente = 1, Anulada = 2 }
public enum ClaseMovimiento { Entrada = 1, Asignacion = 2, Merma = 3 }
public enum EstadoCotizacion { Borrador = 1, Enviada = 2, Aceptada = 3, Rechazada = 4, Facturada = 5, Anulada = 6 }
public enum EstadoFactura { Borrador = 1, Emitida = 2, Pagada = 3, Vencida = 4, Anulada = 5 }
