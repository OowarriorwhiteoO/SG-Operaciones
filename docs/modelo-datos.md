# Modelo de datos

```mermaid
erDiagram
    TipoRegistro ||--o{ Entrada : clasifica
    Entrada ||--o{ Asignacion : origina
    Trabajador ||--o{ Asignacion : recibe
    Entrada ||--o{ Merma : origina
    MotivoMerma ||--o{ Merma : explica
    Entrada {
      int Id PK
      int TipoRegistroId FK
      decimal CantidadInicial
      string DocumentoOrigen UK
      rowversion RowVersion
    }
    Trabajador { int Id PK string Rut UK string NombreCompleto int Estado }
    TipoRegistro { int Id PK string Nombre UK string UnidadMedida int Estado }
    MotivoMerma { int Id PK string Nombre UK bool RequiereEvidencia int Estado }
    Asignacion { int Id PK int EntradaId FK int TrabajadorId FK decimal Cantidad int Estado }
    Merma { int Id PK int EntradaId FK int MotivoMermaId FK decimal Cantidad int Estado }
    Auditoria { long Id PK string Entidad string ClavePrimaria datetime FechaHora }
```

La migración inicial crea restricciones `CHECK`, claves foráneas restrictivas, `rowversion` e índices únicos para RUT, nombres de catálogo y `(DocumentoOrigen, TipoRegistroId)`. Los movimientos están modelados en Sprint 1 para estabilizar el esquema; sus casos de uso se entregan en los siguientes sprints.

