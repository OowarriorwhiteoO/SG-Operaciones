# Despliegue y rollback

## Secuencia controlada

1. Respaldar la base de datos y verificar el respaldo.
2. Revisar migraciones pendientes y generar el script idempotente.
3. Aplicar el script en una ventana controlada.
4. Ejecutar `scripts/deployment/publicar.ps1` o construir la imagen Docker.
5. Publicar la aplicación sin ejecutar migraciones automáticas.
6. Verificar `/health/live`, `/health/ready`, login y una consulta.
7. Habilitar tráfico y monitorear logs/códigos 5xx.

## IIS

Instale el .NET 8 Hosting Bundle, publique con `dotnet publish`, cree un Application Pool sin código administrado, configure HTTPS y entregue la cadena mediante variable `ConnectionStrings__DefaultConnection`.

## Rollback

Conserve el paquete anterior. Ante una falla, retire tráfico, restaure los binarios anteriores y, únicamente si la migración no es compatible hacia atrás, restaure el respaldo validado. Documente el incidente usando el Correlation ID.
