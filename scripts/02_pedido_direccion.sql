-- Script idempotente para bases de datos YA creadas.
-- Agrega la columna donde se guarda la direccion de entrega a domicilio de cada pedido.
USE FinalPasteleria;
GO

IF COL_LENGTH('Pedido', 'direccion') IS NULL
    ALTER TABLE Pedido ADD direccion NVARCHAR(200) NULL;
GO
