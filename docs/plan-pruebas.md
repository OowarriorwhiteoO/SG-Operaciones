# Plan de pruebas

## Cobertura automatizada

- Dominio: cantidades positivas, estados, anulaciones, evidencia y normalización.
- Aplicación: paginación.
- Integración SQL Server: restricciones, auditoría, saldo, concurrencia, mermas, reportes, filtros y exportaciones.
- Web: login visible, redirección anónima, protección por políticas, health checks y páginas de error.

## Prueba de humo

1. `/health/live` y `/health/ready` responden correctamente.
2. Login válido abre el panel; login inválido muestra mensaje sin revelar detalles.
3. Crear entrada, asignación y merma con datos de demostración.
4. Verificar saldo y auditoría.
5. Exportar CSV/PDF y comparar totales.
6. Confirmar que Consulta no puede modificar y que solo Administrador accede a Auditoría.
