# Backlog y avance

## Sprint 1 — completado

- Solución y referencias por capas.
- Entidades, enumeraciones, reglas iniciales y modelo EF.
- Identity, cuatro roles, políticas y administrador de desarrollo seguro.
- CRUD de trabajadores, tipos de registro y motivos de merma.
- Seed idempotente, migración inicial y pruebas automatizadas.

## Sprint 2 — completado

- Registro, consulta, detalle y filtros paginados de entradas.
- Registro y consulta de asignaciones.
- Saldo centralizado reutilizado por consultas, validación y endpoint autorizado.
- Transacción serializable más concurrencia optimista con `rowversion`.
- Auditoría de entrada y asignación dentro de la transacción principal.
- Datos demostrativos con entradas con y sin saldo.
- Pruebas contra SQL Server LocalDB para restricciones, saldo, auditoría y token obsoleto.

## Sprint 3 — completado

- Registro y consulta filtrable de mermas.
- Evidencia obligatoria según configuración del motivo.
- Anulación lógica de entradas, asignaciones y mermas.
- Reposición automática de saldo al anular consumos.
- Rechazo de anulación de entradas con movimientos vigentes.
- Auditoría transaccional de las anulaciones.
- Indicadores por motivo, tipo, frecuencia y porcentaje acumulado Pareto.
- Datos demostrativos con mermas vigentes y anuladas.

## Sprint 4 — completado

- Indicadores interactivos con tasa global, Pareto y navegación al detalle filtrado.
- Consulta combinada y paginada de entradas, asignaciones y mermas.
- Filtros por período, clase, tipo, trabajador, estado, documento y responsable.
- Informe consolidado por tipo con frecuencias, saldos y porcentaje de merma.
- Informe por trabajador con documentos de origen, subtotales y estado.
- Exportaciones CSV y PDF con filtros, período, zona horaria y nombres descriptivos.
- Auditoría persistente de cada descarga.
- Pruebas de cálculos, consulta combinada y generación de archivos.

## Sprint 5 — completado

- Rediseño integral basado en las vistas de referencia de Stitch.
- Dashboard dinámico, auditoría consultable y experiencia responsive.
- Pruebas web, endurecimiento de seguridad y health checks.
- Docker, SQL Server, publicación IIS, rollback y documentación final.

## Pendientes

- Validación con usuarios y preparación de una eventual versión productiva.
