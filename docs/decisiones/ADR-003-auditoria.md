# ADR-003: Auditoría transaccional

**Estado:** aceptado.

La auditoría se modela como tabla append-only y se persiste en la misma transacción del cambio principal. El servicio captura usuario, correlación, IP, agente, acción y valores JSON. Se excluyen contraseñas, hashes, tokens, cookies y cadenas de conexión.
