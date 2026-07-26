# Matriz de permisos

| Capacidad | Administrador | Bodega | Supervisor | Consulta |
|---|---:|---:|---:|---:|
| Usuarios y roles | Sí | No | No | No |
| Trabajadores y catálogos | Sí | Lectura activa | Lectura | Lectura |
| Crear entradas/asignaciones/mermas | Sí | Sí | No | No |
| Consultar operación | Sí | Sí | Sí | Sí |
| Indicadores de merma | Sí | No | Sí | No |
| Anular movimientos | Sí | No | Sí | No |
| Reportes y exportación | Sí | No | Sí | Según política |
| Auditoría | Sí | No | No | No |
| Configuración de empresa | Sí | No | No | No |
| Clientes, productos y servicios | Sí | No | Sí | No |
| Cotizaciones y facturación interna | Sí | No | Sí | No |

Las políticas `AdministrarCatalogos`, `GestionarTrabajadores`, `LecturaOperacional`, `CrearMovimientos`, `AnularMovimientos`, `GenerarReportes` y `GestionComercial` se verifican en servidor. La configuración general de la empresa exige directamente el rol `Administrador`.
