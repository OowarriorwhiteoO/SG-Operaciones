# SG-Operaciones

Aplicación transaccional en ASP.NET Core MVC para centralizar entradas, asignaciones y mermas. Incluye arquitectura por capas, Identity, catálogos, movimientos, saldo, auditoría, concurrencia, anulaciones, indicadores, consultas, informes y exportaciones.

## Requisitos

- .NET 8 SDK o SDK posterior capaz de compilar `net8.0`.
- SQL Server LocalDB, Developer o Express.

## Inicio rápido

```powershell
dotnet tool restore
dotnet restore
$env:SGW_ADMIN_EMAIL="<correo-administrador>"
$env:SGW_ADMIN_PASSWORD="<contraseña-segura>"
dotnet tool run dotnet-ef database update --project src/SistemaGestion.Infrastructure --startup-project src/SistemaGestion.Web
dotnet run --project src/SistemaGestion.Web
```

Las credenciales solo deben vivir en variables de entorno o en el almacén local de secretos; nunca se persisten en el repositorio. En desarrollo también pueden configurarse con User Secrets:

```powershell
dotnet user-secrets init --project src/SistemaGestion.Web
dotnet user-secrets set "SGW_ADMIN_EMAIL" "<correo-administrador>" --project src/SistemaGestion.Web
dotnet user-secrets set "SGW_ADMIN_PASSWORD" "<contraseña-segura>" --project src/SistemaGestion.Web
```

El administrador inicial solo se crea cuando ambas variables están configuradas. Si la base de datos ya contiene usuarios, sus credenciales no se modifican.

## Verificación

```powershell
dotnet build SistemaGestion.slnx
dotnet test SistemaGestion.slnx
```

## Estado

- Sprint 1: completado.
- Sprint 2: completado.
- Sprint 3: completado.
- Sprint 4: completado.
- Sprint 5: completado.

Consulte [arquitectura](docs/arquitectura.md), [modelo de datos](docs/modelo-datos.md), [reglas](docs/reglas-negocio.md), [permisos](docs/matriz-permisos.md), [instalación](docs/manual-instalacion.md), [manual de usuario](docs/manual-usuario.md), [pruebas](docs/plan-pruebas.md) y [despliegue](docs/despliegue.md).
