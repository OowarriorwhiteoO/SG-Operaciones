# Manual de instalación

## Desarrollo local

Requisitos: .NET 8 SDK, SQL Server LocalDB/Developer/Express y la herramienta `dotnet-ef`.

1. Ejecute `dotnet tool restore` y `dotnet restore`.
2. Configure `SGW_ADMIN_EMAIL` y `SGW_ADMIN_PASSWORD` mediante variables de entorno o User Secrets.
3. Ejecute `dotnet tool run dotnet-ef database update --project src/SistemaGestion.Infrastructure --startup-project src/SistemaGestion.Web`.
4. Inicie con `dotnet run --project src/SistemaGestion.Web`.
5. Verifique `/health/ready` e inicie sesión.

Las fechas se almacenan en UTC. La zona de presentación se configura con `System__TimeZoneId`.

## Contenedores

Configure `SGW_SA_PASSWORD` y `SGW_ADMIN_PASSWORD` sin guardarlas en archivos versionados. Ejecute `docker compose up --build`. La aplicación queda disponible en `http://localhost:8080`.

Las migraciones se aplican de manera controlada antes de habilitar tráfico. Genere el SQL idempotente con `scripts/database/generar-script.ps1`.
