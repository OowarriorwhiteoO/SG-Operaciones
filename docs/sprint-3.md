# Sprint 3: mermas, anulaciones e indicadores

## Objetivo

Registrar pérdidas asociadas a una entrada, exigir evidencia cuando corresponda, conservar el historial mediante anulaciones lógicas y entregar análisis agregado de causas.

## Flujo de merma

```mermaid
sequenceDiagram
    actor U as Usuario Bodega
    participant W as Web MVC
    participant S as MermaService
    participant DB as SQL Server
    U->>W: Selecciona entrada y motivo
    W->>S: Crear merma + rowversion
    S->>DB: BEGIN Serializable
    S->>DB: Lee entrada y motivo activos
    S->>S: Valida evidencia y token
    S->>DB: Recalcula saldo vigente
    S->>DB: Inserta merma y actualiza entrada
    S->>DB: Inserta auditoría
    S->>DB: COMMIT
```

## Flujo de anulación

```mermaid
flowchart TD
    A["Solicitud autorizada"] --> B{"RowVersion vigente"}
    B -- No --> C["Rechazar por concurrencia"]
    B -- Sí --> D{"Tipo de movimiento"}
    D -- Entrada --> E{"¿Tiene movimientos vigentes?"}
    E -- Sí --> F["Rechazar: anular movimientos primero"]
    E -- No --> G["Marcar entrada anulada"]
    D -- Asignación --> H["Marcar anulada y reponer saldo"]
    D -- Merma --> I["Marcar anulada y reponer saldo"]
    G --> J["Auditar y confirmar transacción"]
    H --> J
    I --> J
```

## Indicadores

La consulta agrupa exclusivamente mermas vigentes por motivo, tipo y unidad. Calcula cantidad, frecuencia, porcentaje sobre mermas, porcentaje sobre entradas y porcentaje acumulado para Pareto sin cargar entidades completas.

## Verificación

```powershell
dotnet tool run dotnet-ef migrations has-pending-model-changes `
  --project src/SistemaGestion.Infrastructure `
  --startup-project src/SistemaGestion.Web
dotnet build SistemaGestion.slnx
dotnet test SistemaGestion.slnx
dotnet run --project src/SistemaGestion.Web
```

