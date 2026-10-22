-- Crear la base de datos
CREATE DATABASE FinalPasteleria;
GO

--DROP DATABASE FinalPasteleria;

-- Crear login en el servidor
USE master;
GO

CREATE LOGIN usrpasteleria 
WITH PASSWORD = '123456',
     CHECK_POLICY = ON,
     CHECK_EXPIRATION = OFF,
     DEFAULT_DATABASE = FinalPasteleria;
GO

-- Crear usuario en la base de datos
USE FinalPasteleria;
GO

CREATE USER usrpasteleria FOR LOGIN usrpasteleria;
GO

-- Asignar rol de db_owner al usuario
ALTER ROLE db_owner ADD MEMBER usrpasteleria;
GO

-- Eliminar tablas si existen
DROP TABLE IF EXISTS CarritoDetalle;
DROP TABLE IF EXISTS Carrito;
DROP TABLE IF EXISTS SeguimientoPedido;
DROP TABLE IF EXISTS DetallePedido;
DROP TABLE IF EXISTS TokenSeguimiento;
DROP TABLE IF EXISTS Pedido;
DROP TABLE IF EXISTS MetodoPago;
DROP TABLE IF EXISTS Producto;
DROP TABLE IF EXISTS CategoriaProducto;
DROP TABLE IF EXISTS Cliente;
DROP TABLE IF EXISTS Usuario;
DROP TABLE IF EXISTS Empleado;


-- =============================================
-- 1. Empleado
-- =============================================
CREATE TABLE Empleado (
    id INT IDENTITY(1,1) PRIMARY KEY,
    cedulaIdentidad VARCHAR(12) NOT NULL UNIQUE,
    nombre NVARCHAR(50) NOT NULL,
    apellidoPaterno NVARCHAR(50) NOT NULL,
    apellidoMaterno NVARCHAR(50),
    fechaNacimiento DATE NOT NULL,
    direccion NVARCHAR(200) NOT NULL,
    celular VARCHAR(20) NOT NULL,
    email NVARCHAR(100) NOT NULL,
    cargo NVARCHAR(50) NOT NULL,
    usuarioRegistro NVARCHAR(50) NOT NULL DEFAULT SUSER_NAME(),
    fechaRegistro DATETIME NOT NULL DEFAULT GETDATE(),
    estado TINYINT NOT NULL DEFAULT 1 -- -1 eliminado, 0 inactivo, 1 activo
);

-- =============================================
-- 2. Usuario
-- =============================================
CREATE TABLE Usuario (
    id INT IDENTITY(1,1) PRIMARY KEY,
    idEmpleado INT NOT NULL,
    usuario NVARCHAR(50) NOT NULL UNIQUE,
    clave NVARCHAR(255) NOT NULL,
    role NVARCHAR(50) NOT NULL,
    ultimoLogin DATETIME,
    intentosFallidos INT DEFAULT 0,
    usuarioRegistro NVARCHAR(50) NOT NULL DEFAULT SUSER_NAME(),
    fechaRegistro DATETIME NOT NULL DEFAULT GETDATE(),
    estado TINYINT NOT NULL DEFAULT 1 -- -1 eliminado, 0 inactivo, 1 activo
    CONSTRAINT FK_Usuario_Empleado FOREIGN KEY (idEmpleado)
        REFERENCES Empleado(id)
);

-- =============================================
-- 3. Cliente
-- =============================================
CREATE TABLE Cliente (
    id INT IDENTITY(1,1) PRIMARY KEY,
    cedulaIdentidad VARCHAR(12) UNIQUE NOT NULL,
    nombre NVARCHAR(50) NOT NULL,
    apellido NVARCHAR(50) NOT NULL,
    celular VARCHAR(20),
    email NVARCHAR(100),
    direccion NVARCHAR(200),
    tipoCliente NVARCHAR(50),
    preferencias NVARCHAR(500),
    usuarioRegistro NVARCHAR(50) DEFAULT SUSER_NAME(),
    fechaRegistro DATETIME NOT NULL DEFAULT GETDATE(),
    estado TINYINT NOT NULL DEFAULT 1 -- -1 eliminado, 0 inactivo, 1 activo
);

-- =============================================
-- 4. CategoriaProducto
-- =============================================
CREATE TABLE CategoriaProducto (
    id INT IDENTITY(1,1) PRIMARY KEY,
    nombre NVARCHAR(50) NOT NULL,
    estado SMALLINT NOT NULL DEFAULT 1
);

-- =============================================
-- 5. Producto
-- =============================================
CREATE TABLE Producto (
    id INT IDENTITY(1,1) PRIMARY KEY,
    codigo NVARCHAR(50) UNIQUE,
    nombre NVARCHAR(100) NOT NULL,
    idCategoria INT NOT NULL,
    unidadMedida NVARCHAR(20),
    precio DECIMAL(12,2) NOT NULL,
    stock INT DEFAULT 0,
    tipo NVARCHAR(50),
    descripcion NVARCHAR(500),
    imagenUrl NVARCHAR(500),
    usuarioRegistro NVARCHAR(50) NOT NULL DEFAULT SUSER_NAME(),
    fechaRegistro DATETIME NOT NULL DEFAULT GETDATE(),
    estado TINYINT NOT NULL DEFAULT 1 -- -1 eliminado, 0 inactivo, 1 activo
    CONSTRAINT FK_Producto_Categoria FOREIGN KEY (idCategoria)
        REFERENCES CategoriaProducto(id)
);

-- =============================================
-- 6. MetodoPago
-- =============================================
CREATE TABLE MetodoPago (
    id INT IDENTITY(1,1) PRIMARY KEY,
    nombre NVARCHAR(50) NOT NULL
);

-- =============================================
-- 7. Pedido
-- =============================================
CREATE TABLE Pedido (
    id INT IDENTITY(1,1) PRIMARY KEY,
    codigoSeguimiento NVARCHAR(50) UNIQUE,
    fechaPedido DATETIME DEFAULT GETDATE(),
    fechaEntrega DATETIME,
    idCliente INT NULL,
    nombreCliente NVARCHAR(100),
    emailCliente NVARCHAR(100),
    celularCliente VARCHAR(20),
    idUsuario INT NOT NULL,
    tipoEntrega NVARCHAR(50),
    metodoPago INT,
    observaciones NVARCHAR(500),
    total DECIMAL(12,2) DEFAULT 0,
    estadoPedido TINYINT DEFAULT 0,
    usuarioRegistro NVARCHAR(50) NOT NULL DEFAULT SUSER_NAME(),
    fechaRegistro DATETIME NOT NULL DEFAULT GETDATE(),
    estado TINYINT NOT NULL DEFAULT 1 -- -1 eliminado, 0 inactivo, 1 activo
    CONSTRAINT FK_Pedido_Cliente FOREIGN KEY (idCliente)
        REFERENCES Cliente(id),
    CONSTRAINT FK_Pedido_Usuario FOREIGN KEY (idUsuario)
        REFERENCES Usuario(id),
    CONSTRAINT FK_Pedido_MetodoPago FOREIGN KEY (metodoPago)
        REFERENCES MetodoPago(id)
);

-- =============================================
-- 8. DetallePedido
-- =============================================
CREATE TABLE DetallePedido (
    id INT IDENTITY(1,1) PRIMARY KEY,
    idPedido INT NOT NULL,
    idProducto INT NOT NULL,
    cantidad INT NOT NULL,
    precioUnitario DECIMAL(12,2) NOT NULL,
    descuento DECIMAL(12,2) DEFAULT 0,
    subTotal AS (cantidad * precioUnitario - descuento) PERSISTED,
    estadoDetalle TINYINT DEFAULT 1,
    usuarioRegistro NVARCHAR(50) NOT NULL DEFAULT SUSER_NAME(),
    fechaRegistro DATETIME NOT NULL DEFAULT GETDATE(),
    estado TINYINT NOT NULL DEFAULT 1 -- -1 eliminado, 0 inactivo, 1 activo
    CONSTRAINT FK_DetallePedido_Pedido FOREIGN KEY (idPedido)
        REFERENCES Pedido(id),
    CONSTRAINT FK_DetallePedido_Producto FOREIGN KEY (idProducto)
        REFERENCES Producto(id)
);

-- =============================================
-- 9. SeguimientoPedido
-- =============================================
CREATE TABLE SeguimientoPedido (
    id INT IDENTITY(1,1) PRIMARY KEY,
    idPedido INT NOT NULL,
    estadoPedido TINYINT NOT NULL,
    fecha DATETIME DEFAULT GETDATE(),
    descripcion NVARCHAR(500),
    usuarioRegistro NVARCHAR(50) NOT NULL DEFAULT SUSER_NAME(),
    CONSTRAINT FK_SeguimientoPedido_Pedido FOREIGN KEY (idPedido)
        REFERENCES Pedido(id)
);

-- =============================================
-- 10. Carrito
-- =============================================
CREATE TABLE Carrito (
    id INT IDENTITY(1,1) PRIMARY KEY,
    idCliente INT NULL,
    fecha DATETIME DEFAULT GETDATE(),
    usuarioRegistro NVARCHAR(50) NOT NULL DEFAULT SUSER_NAME(),
    CONSTRAINT FK_Carrito_Cliente FOREIGN KEY (idCliente)
        REFERENCES Cliente(id)
);

-- =============================================
-- 11. CarritoDetalle
-- =============================================
CREATE TABLE CarritoDetalle (
    id INT IDENTITY(1,1) PRIMARY KEY,
    idCarrito INT NOT NULL,
    idProducto INT NOT NULL,
    cantidad INT NOT NULL,
    precioUnitario DECIMAL(12,2) NOT NULL,
    subTotal AS (cantidad * precioUnitario) PERSISTED,
    CONSTRAINT FK_CarritoDetalle_Carrito FOREIGN KEY (idCarrito)
        REFERENCES Carrito(id),
    CONSTRAINT FK_CarritoDetalle_Producto FOREIGN KEY (idProducto)
        REFERENCES Producto(id)
);

-- Agregar columna para clientes invitados
ALTER TABLE Cliente 
ADD esInvitado BIT DEFAULT 0;

-- Tabla de tokens para seguimiento sin login
CREATE TABLE TokenSeguimiento (
    id INT IDENTITY(1,1) PRIMARY KEY,
    idPedido INT NOT NULL,
    token NVARCHAR(100) UNIQUE NOT NULL,
    fechaExpiracion DATETIME NOT NULL,
    usado BIT DEFAULT 0,
    CONSTRAINT FK_Token_Pedido FOREIGN KEY (idPedido)
        REFERENCES Pedido(id)
);

-- Mejorar tabla SeguimientoPedido para admin
ALTER TABLE SeguimientoPedido
ADD notasInternas NVARCHAR(500);

-- INSERT INTO

INSERT INTO Empleado (cedulaIdentidad, nombre, apellidoPaterno, apellidoMaterno, fechaNacimiento, direccion, celular, email, cargo)
VALUES ('23184952', 'Magin', 'Condori', 'Huanca', '2000-01-15', 'Calle españa', '78590021', 'condori@gmail.com', 'Administrador');

INSERT INTO Usuario(idEmpleado,usuario,clave, role)
VALUES (1, 'condori', 'I0HCOO/NSSY6WOS9POP5XW==', 'Administrador'); -- Clave: hola123

INSERT INTO Cliente (cedulaIdentidad, nombre, apellido, celular, email, direccion, tipoCliente)
VALUES
('20002000', 'Pedro', 'Mendoza', '70001122', 'pedro.m@mail.com', 'Zona Sur, Edif. Apto. 1', 'VIP'),
('6574732', 'Ana', 'Sánchez', '71003344', 'ana.s@mail.com', 'Barrio Norte, Casa 5', 'Regular');

INSERT INTO CategoriaProducto (nombre)
VALUES
('Torta'),
('Pay'),
('Pastel'),
('Cupcake');

INSERT INTO Producto (codigo, nombre, idCategoria, unidadMedida, precio, stock, tipo, descripcion, imagenUrl)
VALUES
('PST001', 'Torta de Chocolate', 1, 'Unidad', 80.50, 50, 'Principal', 'Torta mediana para 10 personas.', 'https://www.nestleprofessional-latam.com/sites/default/files/styles/np_recipe_detail/public/2024-08/torta-barras-chocolate-black-white-sublime.jpg?itok=BXcporU-'),
('PST002', 'Mini Cupcake Vainilla', 1, 'Docena', 45.00, 100, 'Secundario', 'Cupcakes de vainilla con crema.', 'https://i0.wp.com/sarasellos.com/wp-content/uploads/2024/05/cupcakes-vainilla-1.jpg?resize=779%2C779&ssl=1'),
('BEB001', 'Jugo Natural Naranja', 2, 'Litro', 15.00, 200, 'Principal', 'Jugo recién exprimido.', '');

UPDATE Producto
SET imagenUrl = 'https://5aldia.cl/wp-content/uploads/2018/04/jugo-de-naranja-destacado.jpg'
WHERE codigo = 'BEB001';

INSERT INTO MetodoPago (nombre)
VALUES
('Efectivo'),
('QR');


SELECT * FROM Usuario;

-- =============================================
-- 1. paListarProductos (ACTUALIZADO: Agrega nombre de Categoría)
-- =============================================
DROP PROCEDURE IF EXISTS paListarProductos;
GO

-- Listar Productos con su Categoría y Activos
CREATE PROC paListarProductos @parametro VARCHAR(50)
AS
    SELECT
        P.id,
        P.codigo,
        P.nombre,
        C.nombre AS CategoriaNombre, -- Nuevo: Nombre de la categoría
        P.unidadMedida,
        P.precio,
        P.stock, -- Incluir el stock
        P.tipo,
        P.descripcion,
        P.estado,
        P.usuarioRegistro,
        P.fechaRegistro
    FROM Producto P
    INNER JOIN CategoriaProducto C ON P.idCategoria = C.id -- Unión con Categoría
    WHERE P.estado = 1 -- Solo productos Activos
    AND (
        P.nombre LIKE '%' + @parametro + '%' -- Búsqueda por nombre
        OR P.codigo LIKE '%' + @parametro + '%' -- Búsqueda por código
    )
    ORDER BY P.nombre ASC;
GO

-- Ejecutar el procedimiento almacenado
EXEC paListarProductos '';
GO

-- =============================================
-- 2. paListarEmpleados (SIN CAMBIOS Mayores - Uso de LEFT JOIN es correcto)
-- =============================================
DROP PROC IF EXISTS paListarEmpleados;
GO
CREATE PROC paListarEmpleados @parametro VARCHAR(50)
AS
    SELECT
        E.id,
        E.cedulaIdentidad,
        E.nombre,
        E.apellidoPaterno,
        E.apellidoMaterno,
        E.fechaNacimiento,
        E.direccion,
        E.celular,
        E.cargo, -- Se agregó cargo (estaba en la tabla Empleado)
        U.role,
        -- El usuario de la tabla Usuario (U.usuario)
        ISNULL(U.usuario, 'Sin Usuario') AS 'UsuarioAsignado', 
        E.estado,
        E.usuarioRegistro,
        E.fechaRegistro
    FROM 
        Empleado E
    LEFT JOIN 
        Usuario U ON E.id = U.idEmpleado
    WHERE 
        E.estado = 1 -- Solo empleados Activos
        AND (
            E.nombre LIKE '%' + @parametro + '%' 
            OR E.apellidoPaterno LIKE '%' + @parametro + '%'
            OR E.apellidoMaterno LIKE '%' + @parametro + '%'
            OR E.cedulaIdentidad LIKE '%' + @parametro + '%'
        )
    ORDER BY 
        E.apellidoPaterno ASC, E.nombre ASC;
GO

-- Ejecutar el procedimiento almacenado
EXEC paListarEmpleados '';
GO

-- =============================================
-- 3. paListarCliente (SIN CAMBIOS Mayores - Uso de CONCATENACION para busqueda es correcto)
-- =============================================
DROP PROC IF EXISTS paListarCliente;
GO

-- Listar Clientes Activos con Búsqueda
CREATE PROC paListarCliente @parametro VARCHAR(50)
AS
    SELECT
        id,
        cedulaIdentidad,
        nombre,
        apellido,
        celular,
        email,
        direccion,
        tipoCliente, -- Agregado tipoCliente
        preferencias, -- Agregado preferencias
        usuarioRegistro,
        fechaRegistro,
        estado
    FROM Cliente
    WHERE estado = 1 -- Solo Clientes Activos
    AND (
        nombre + ' ' + apellido LIKE '%' + @parametro + '%' OR
        email LIKE '%' + @parametro + '%' OR
        cedulaIdentidad LIKE '%' + @parametro + '%'
    )
    ORDER BY apellido ASC;
GO

EXEC paListarCliente 'juan';
GO

-- =============================================
-- 4. paListarPedidos (MEJORADO: Manejo de clientes NULL/Registrados y MetodoPago)
-- =============================================
DROP PROC IF EXISTS paListarPedidos;
GO
CREATE PROCEDURE paListarPedidos (@parametro VARCHAR(50) = NULL)
AS
BEGIN
    SELECT 
        P.id,
        P.codigoSeguimiento,
        P.fechaPedido,
        P.fechaEntrega,
        -- Combina datos de Cliente registrado (C) o datos de cliente invitado (P)
        CASE WHEN P.idCliente IS NOT NULL THEN C.nombre + ' ' + ISNULL(C.apellido, '') ELSE P.nombreCliente END AS NombreCliente,
        CASE WHEN P.idCliente IS NOT NULL THEN C.celular ELSE P.celularCliente END AS TelefonoCliente,
        MP.nombre AS MetodoPago, -- Agregado MetodoPago
        P.total,
        P.tipoEntrega,
        P.usuarioRegistro,
        -- Mapeo de estadoPedido a un nombre legible
        CASE P.estadoPedido
            WHEN 0 THEN 'PENDIENTE'
            WHEN 1 THEN 'CONFIRMADO' -- Asumiendo 1 = Confirmado/Procesando
            WHEN 2 THEN 'ENTREGADO'  -- Asumiendo 2 = Entregado
            ELSE 'OTRO'
        END AS EstadoNegocio,
        P.estadoPedido AS EstadoCodigo -- Devolvemos el código para filtros

    FROM Pedido P
    LEFT JOIN Cliente C ON P.idCliente = C.id -- LEFT JOIN para clientes no registrados
    INNER JOIN MetodoPago MP ON P.metodoPago = MP.id
    WHERE P.estado = 1 -- Pedidos activos
      AND (@parametro IS NULL OR 
           P.codigoSeguimiento LIKE '%' + @parametro + '%' OR
           CASE WHEN P.idCliente IS NOT NULL THEN C.nombre + ' ' + ISNULL(C.apellido, '') ELSE P.nombreCliente END LIKE '%' + @parametro + '%'
          )
    ORDER BY P.fechaPedido DESC;
END
GO

EXEC paListarPedidos @parametro = NULL; -- Listar todos
-- EXEC paListarPedidos @parametro = '123456'; -- Buscar por código de seguimiento o cliente
GO

-- =============================================
-- 5. paListarDetallePedido (CORREGIDO: Eliminado d.metodoPago y uso de subTotal Persistido)
-- =============================================
DROP PROCEDURE IF EXISTS paListarDetallePedido;
GO

CREATE PROCEDURE paListarDetallePedido
    @idPedido INT
AS
BEGIN
    SELECT 
        d.id,
        p.nombre AS Producto,
        d.cantidad,
        d.precioUnitario,
        d.descuento, -- Incluir el descuento
        d.subTotal, -- Usar la columna PERSISTED (cantidad * precioUnitario - descuento)
        d.usuarioRegistro,
        d.fechaRegistro,
        d.estado,

        -- Información del Pedido (Para contexto)
        CASE ped.estadoPedido
            WHEN 0 THEN 'PENDIENTE'
            WHEN 1 THEN 'CONFIRMADO'
            WHEN 2 THEN 'ENTREGADO'
            ELSE 'OTRO'
        END AS EstadoNegocio,
        ped.fechaEntrega AS FechaEntrega
    FROM DetallePedido d
    INNER JOIN Producto p ON p.id = d.idProducto
    INNER JOIN Pedido ped ON ped.id = d.idPedido
    WHERE d.idPedido = @idPedido AND d.estado = 1
    ORDER BY d.id ASC;
END
GO

EXEC paListarDetallePedido @idPedido = 1; 
GO

-- =============================================
-- 6. paReportePedidos (CORREGIDO: Elimina lógica de GANANCIA (Producto no tiene costo) y simplifica TOTAL)
-- =============================================
DROP PROCEDURE IF EXISTS paReportePedidos;
GO

CREATE PROCEDURE paReportePedidos
(
    -- Parametros de filtrado
    @FechaInicio DATE,
    @FechaFin DATE,
    @EstadoPedido TINYINT = NULL, -- Nuevo filtro por estado del pedido
    @NombreCliente VARCHAR(200) = NULL 
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        -- 1. Informacion del Pedido
        P.id AS IdPedido,
        P.codigoSeguimiento,
        P.fechaPedido AS FechaPedido,
        P.fechaEntrega AS FechaEntrega,
        MP.nombre AS MetodoPago,
        P.tipoEntrega AS TipoEntrega,
        P.estadoPedido AS EstadoPedido,
        P.observaciones AS Observaciones,
        P.total AS MontoTotal, -- Usar el total ya calculado en la cabecera del Pedido

        -- 2. Informacion del Cliente
        CASE WHEN P.idCliente IS NOT NULL THEN C.nombre + ' ' + ISNULL(C.apellido, '') ELSE P.nombreCliente END AS NombreCliente,
        CASE WHEN P.idCliente IS NOT NULL THEN C.celular ELSE P.celularCliente END AS CelularCliente,
        CASE WHEN P.idCliente IS NOT NULL THEN C.direccion ELSE NULL END AS DireccionCliente,

        -- 3. Informacion del Empleado (Usuario que registró)
        E.nombre + ' ' + E.apellidoPaterno AS NombreEmpleado,

        -- 4. Detalle de Productos (Para reportes detallados)
        (
            SELECT 
                DP.cantidad, 
                PR.nombre AS Producto, 
                DP.precioUnitario, 
                DP.descuento,
                DP.subTotal -- SubTotal de cada ítem
            FROM DetallePedido DP
            INNER JOIN Producto PR ON DP.idProducto = PR.id
            WHERE DP.idPedido = P.id
            FOR JSON PATH
        ) AS DetalleProductosJSON

    FROM Pedido P
    LEFT JOIN Cliente C ON P.idCliente = C.id -- LEFT JOIN para manejar pedidos sin idCliente
    INNER JOIN Usuario U ON P.idUsuario = U.id
    INNER JOIN Empleado E ON U.idEmpleado = E.id
    INNER JOIN MetodoPago MP ON P.metodoPago = MP.id

    WHERE
        P.fechaPedido >= @FechaInicio 
        AND P.fechaPedido < DATEADD(DAY, 1, @FechaFin) -- Rango inclusivo
        AND P.estado = 1 -- Solo pedidos activos
        AND (@EstadoPedido IS NULL OR P.estadoPedido = @EstadoPedido) -- Filtro opcional por estado
        AND (@NombreCliente IS NULL 
              OR (CASE WHEN P.idCliente IS NOT NULL THEN C.nombre + ' ' + ISNULL(C.apellido, '') ELSE P.nombreCliente END LIKE '%' + @NombreCliente + '%') 
              OR C.cedulaIdentidad LIKE '%' + @NombreCliente + '%') 
    
    ORDER BY 
        P.fechaPedido DESC;

END
GO

-- Ejemplo de ejecución del procedimiento almacenado para reporte de pedidos (Ahora usa el TOTAL de la tabla Pedido)
EXEC paReportePedidos
    @FechaInicio = '2024-01-01',
    @FechaFin = '2025-12-31',
    @EstadoPedido = NULL, -- Opcional: Filtrar por un estado específico (ej. 2 para Entregado)
    @NombreCliente = NULL;
GO

-- =============================================
-- 7. paListarUsuarios
-- =============================================
CREATE PROCEDURE paListarUsuarios
    @filtro NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        u.id,
        u.usuario,
        u.role,
        u.estado,
        u.ultimoLogin,
        u.intentosFallidos,
        u.fechaRegistro,
        e.nombre AS nombreEmpleado,
        e.apellidoPaterno,
        e.apellidoMaterno,
        e.cedulaIdentidad
    FROM Usuario u
    INNER JOIN Empleado e ON u.idEmpleado = e.id
    WHERE 
        u.usuario LIKE '%' + @filtro + '%' OR
        u.role LIKE '%' + @filtro + '%' OR
        e.nombre LIKE '%' + @filtro + '%' OR
        e.apellidoPaterno LIKE '%' + @filtro + '%' OR
        e.cedulaIdentidad LIKE '%' + @filtro + '%'
    ORDER BY u.id DESC;
END;
GO
