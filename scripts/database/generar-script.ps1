param([string]$Output = "output/database/migraciones.sql")
$directory = Split-Path -Parent $Output
New-Item -ItemType Directory -Force -Path $directory | Out-Null
dotnet tool restore
dotnet tool run dotnet-ef migrations script --idempotent `
  --project src/SistemaGestion.Infrastructure `
  --startup-project src/SistemaGestion.Web `
  --output $Output
if ($LASTEXITCODE -ne 0) { throw "No fue posible generar el script de migraciones." }
Write-Host "Script generado en $Output"
