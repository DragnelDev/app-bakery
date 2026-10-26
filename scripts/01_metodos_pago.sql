-- Script idempotente para bases de datos YA creadas.
-- El checkout ahora lista los metodos de pago desde esta tabla; este script agrega los que falten.
USE FinalPasteleria;
GO

INSERT INTO MetodoPago (nombre)
SELECT v.nombre
FROM (VALUES ('Efectivo'), ('QR'), ('Tarjeta'), ('Transferencia')) AS v(nombre)
WHERE NOT EXISTS (SELECT 1 FROM MetodoPago m WHERE m.nombre = v.nombre);
GO
