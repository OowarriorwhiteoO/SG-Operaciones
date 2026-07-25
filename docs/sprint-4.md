# Sprint 4 — consultas, informes e indicadores operativos

## Funcionalidad entregada

- El módulo **Indicadores** aplica período y tipo, calcula tasa global, distribución por motivo y Pareto.
- Cada barra y fila abre el listado de mermas con el mismo período, motivo, tipo y estado vigente.
- Descarga de indicadores en CSV y PDF.
- Consulta transversal paginada de entradas, asignaciones y mermas.
- Informe por tipo con entradas, asignaciones, mermas, saldo, frecuencias y porcentaje de merma.
- Informe por trabajador con asignaciones, entrada/documento de origen, subtotales y total vigente.
- Descargas CSV/PDF autorizadas para Administrador y Supervisor.
- Registro de auditoría para cada exportación.

## Criterios técnicos

- Consultas de solo lectura con `AsNoTracking`.
- Filtros aplicados antes de ordenar y paginar.
- `Skip` y `Take` ejecutados por SQL Server en la consulta transversal.
- Fechas de término tratadas como límite exclusivo del día siguiente.
- División por cero controlada en los porcentajes.
- Las mermas no se atribuyen a trabajadores porque el modelo no contiene esa relación.
- CSV UTF-8 con BOM y separador compatible con Excel en configuración regional chilena.
- PDF generado con QuestPDF y licencia Community configurada una vez al iniciar.

## Verificación

- Compilación completa sin errores ni advertencias.
- Pruebas de dominio y aplicación aprobadas.
- Pruebas SQL Server de cálculo por tipo y consulta de los tres movimientos aprobadas.
- Pruebas de CSV/PDF aprobadas.
- PDF de demostración renderizado e inspeccionado visualmente.
