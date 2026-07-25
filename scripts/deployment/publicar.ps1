param([string]$Output = "output/publish")
dotnet restore SistemaGestion.slnx
if ($LASTEXITCODE -ne 0) { throw "Falló la restauración." }
dotnet test SistemaGestion.slnx --no-restore
if ($LASTEXITCODE -ne 0) { throw "Las pruebas no fueron aprobadas." }
dotnet publish src/SistemaGestion.Web/SistemaGestion.Web.csproj -c Release -o $Output --no-restore
if ($LASTEXITCODE -ne 0) { throw "Falló la publicación." }
Write-Host "Publicación disponible en $Output"
