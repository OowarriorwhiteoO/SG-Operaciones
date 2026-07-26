# Módulo comercial

## Alcance

El módulo añade un ciclo comercial básico a SG-Operaciones:

1. El administrador configura la empresa y mantiene trabajadores y usuarios.
2. Administrador o supervisor registra clientes, productos y servicios.
3. Se crea una cotización en borrador con precios, descuentos e IVA.
4. La cotización puede marcarse como enviada, aceptada o rechazada.
5. Una cotización aceptada puede convertirse una sola vez en factura interna.
6. La factura conserva una copia de sus líneas y permite registrar el pago.

Los cambios de estado relevantes quedan trazados en auditoría. Los catálogos se desactivan de forma lógica para preservar el historial.

## Facturación

El documento emitido por SG-Operaciones sirve para control comercial, impresión y cobranza interna. No constituye una factura electrónica ni un documento tributario electrónico autorizado por el Servicio de Impuestos Internos.

Para facturación tributaria real se requiere integrar un proveedor de DTE o implementar el flujo de certificación, folios, firma, envío y seguimiento exigido por el SII.

## Permisos

- `Administrador`: acceso completo, incluida la configuración de empresa.
- `Supervisor`: clientes, catálogo comercial, cotizaciones y facturas.
- `Bodega` y `Consulta`: sin acceso al módulo comercial.

## Datos iniciales

En desarrollo se crean una empresa, clientes, productos y servicios ficticios, además de una cotización de demostración. La carga es idempotente y no contiene credenciales.
