# Reglas de negocio

- Los catálogos se activan o desactivan; nunca se eliminan físicamente.
- RUT y nombres de catálogo son únicos.
- Una entrada exige cantidad positiva, tipo y documento normalizado (`Trim` y mayúsculas).
- Saldo: `CantidadInicial - AsignacionesVigentes - MermasVigentes`.
- Las cantidades se representan con `decimal(18,3)`.
- `RowVersion` es el token de concurrencia para catálogos y movimientos.
- Una operación administrativa requiere autenticación y política de autorización de servidor.
- Una asignación requiere entrada vigente, trabajador activo, cantidad positiva y saldo suficiente.
- El cliente envía el `rowversion` observado, pero el servidor vuelve a leer la entrada y recalcula el saldo.
- La creación de asignaciones usa aislamiento `Serializable`; una actualización de `FechaUltimoMovimiento` provoca un nuevo `rowversion`.
- La entrada y su auditoría, o la asignación y su auditoría, se confirman o revierten como una unidad.
- Una merma exige entrada vigente, motivo activo, cantidad positiva y saldo suficiente.
- Cuando el motivo lo configura, la referencia de evidencia es obligatoria en cliente, aplicación y dominio.
- Anular una asignación o merma la excluye del saldo sin borrar su historial.
- Una entrada no puede anularse mientras conserve asignaciones o mermas vigentes.
- Una anulación requiere motivo, usuario autorizado y `rowversion` vigente; no puede repetirse.

Los indicadores consideran únicamente movimientos vigentes y evitan división por cero.
