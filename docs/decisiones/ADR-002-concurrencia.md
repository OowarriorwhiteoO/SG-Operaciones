# ADR-002: Concurrencia optimista

**Estado:** aceptado.

SQL Server `rowversion` detecta escrituras concurrentes. Los consumos de saldo combinan transacción `Serializable`, recálculo en servidor y actualización de `FechaUltimoMovimiento`. Un token obsoleto se rechaza antes de insertar el movimiento y `DbUpdateConcurrencyException` revierte toda la transacción.
