# Manual de administración

- Cree usuarios y asigne únicamente los roles necesarios.
- Mantenga catálogos activos solo mientras puedan utilizarse en nuevas operaciones.
- Revise periódicamente el Centro de Auditoría, especialmente accesos denegados y exportaciones.
- Cambie la contraseña administrativa inicial después del primer inicio.
- Nunca almacene secretos en `appsettings*.json`.
- Compruebe diariamente `/health/ready`, espacio de base de datos y logs.
- Respalde SQL Server antes de aplicar migraciones.
