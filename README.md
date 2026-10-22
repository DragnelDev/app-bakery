# 🏆 sis457_Pasteleria - Proyecto Final SIS457 (Pastelería "Carmen")

<p align="center">
    <img src="./assets/logo.jpg" alt="Logo de la Pasteleria Carmen" width="200" style="border-radius: 50%; object-fit: cover; aspect-ratio: 1/1;"/>
</p>

## 📌 Requisitos del Trabajo Final (Laboratorio SIS457)

Este proyecto desarrolla un **Sistema de Gestión de Ventas y E-commerce** para la pastelería **"Carmen"**, siguiendo la arquitectura de 3 capas y utilizando **ASP.NET Core MVC**.

### 🏗️ Estructura de la Solución

| Componente | Nombre Requerido (Convención) | Rol en el Sistema |
| :--- | :--- | :--- |
| **Solución** | `Sis457Pasteleria` | Contenedor principal de los proyectos. |
| **Base de Datos** | `FinalPasteleria` | Nombre del script DDL y la base de datos. |
| **Capa Web (Presentación)** | `WebPasteleria` | **ASP.NET Core MVC** (Login, Vistas, Template personalizado). |
| **Capa Lógica (Negocio)** | `ClnPasteleria` | Reglas de negocio y validaciones. |
| **Capa Acceso a Datos (CAD)** | `CadPasteleria` | Entity Framework Core (Interacción con `FinalPasteleria`). |

### ✅ Requisitos Funcionales Implementados

| Requisito | Implementación | Observaciones |
| :--- | :--- | :--- |
| **Interfaz Web** | **WebPasteleria** | Template **Bootstrap Personalizado** adaptado al negocio de pastelería. |
| **Login** | `AccountController` | Autenticación basada en la tabla `Usuario`. |
| **CRUD 1 (E. Lógica)** | `ClienteController` | CRUD completo para la gestión de clientes (incluye **Eliminación Lógica**). |
| **CRUD 2 (E. Lógica)** | `ProductoController` | CRUD para el catálogo de productos (incluye **Eliminación Lógica**). |
| **Funcionalidad Mayor** | **Compra / Venta** | Flujo completo de E-commerce: Carrito -> Checkout -> Generación de Pedido y Detalle. |
| **Validaciones** | Data Annotations en ViewModels | Validaciones de campos vacíos implementadas en la Capa Web y Lógica. |
| **Auditoría** | Campos `usuarioRegistro`, `fechaRegistro` | Trazabilidad en todas las tablas maestras y de transacción. |
| **Repositorio** | `sis457_pasteleria` | Historial de **commits** regulares y consistentes. |

---

## 📋 Modelo de Datos (FinalPasteleria)

La base de datos incluye **11 entidades** diseñadas para el control administrativo, la trazabilidad logística y el soporte del E-commerce.

### Detalle de Tablas y Columnas Clave

| Entidad | Propósito | Columnas Clave | Clave para el Negocio | E. Lógica |
| :--- | :--- | :--- | :--- | :--- |
| **1. Empleado** | Personal administrativo. | `id`, `cedulaIdentidad`, `nombre`, `apellidoPaterno`, `fechaNacimiento`, `cargo`, `estado` | Base para `Usuario` | **Sí** |
| **2. Usuario** | Seguridad y Acceso. | `id`, `idEmpleado` (FK), `usuario`, `clave`, `role`, `ultimoLogin`, `intentosFallidos`, `estado` | Módulo de **Login** | **Sí** |
| **3. Cliente** | Contactos de Compradores. | `id`, `nombre`, `apellido`, `celular`, `email`, `direccion`, `tipoCliente`, `preferencias`, `estado` | **CRUD Obligatorio** | **Sí** |
| **4. CategoriaProducto** | Clasificación de inventario. | `id`, `nombre`, `descripcion` | Filtros del Catálogo | **Sí** |
| **5. Producto** | Catálogo y Stock. | `id`, `codigo`, `nombre`, `idCategoria` (FK), `precio`, `stock`, `imagenUrl`, `estado` | **CRUD Obligatorio** y E-commerce | **Sí** |
| **6. MetodoPago** | Formas de Pago. | `id`, `nombre` | Usado en el Checkout | **Sí** |
| **7. Pedido** | Cabecera de la Venta. | `id`, `codigoSeguimiento`, `idCliente` (FK), `idUsuario` (FK), `fechaEntrega`, `metodoPago` (FK), `total`, `estadoPedido`, `estado` | **Funcionalidad Principal** | **Sí** |
| **8. DetallePedido** | Registro de ítems vendidos. | `id`, `idPedido` (FK), `idProducto` (FK), `cantidad`, `precioUnitario`, `descuento`, `subTotal` (Calculado), `estado` | Detalle de la Venta | **Sí** |
| **9. SeguimientoPedido** | Control Logístico. | `id`, `idPedido` (FK), `estadoPedido`, `fecha`, `descripcion` | Trazabilidad del pedido | **No** (Auditoría) |
| **10. Carrito** | Sesión de Compra Web. | `id`, `idCliente` (FK), `fecha` | Flujo de E-commerce | **No** |
| **11. CarritoDetalle** | Ítems en la sesión. | `id`, `idCarrito` (FK), `idProducto` (FK), `cantidad`, `precioUnitario`, `subTotal` (Calculado) | Flujo de E-commerce | **No** |

---

## 🚀 Flujo de Compra / Venta (Funcionalidad Principal)

La función de mayor valor es el proceso de Venta, que inicia en la interfaz web del cliente y se consolida en la base de datos.

1.  **Exploración:** Cliente navega en el catálogo de productos (Controlador `CatalogoController`).
2.  **Carrito:** El cliente agrega productos. Se persiste en las tablas `Carrito` y `CarritoDetalle` (Controlador `CarritoController`).
3.  **Checkout:** El cliente finaliza, confirmando datos de entrega y `MetodoPago`.
4.  **Generación de Pedido:** La Lógica de Negocio (`ClnPasteleria`) valida y transforma los datos del `Carrito` en un nuevo registro en `Pedido` y varios registros en `DetallePedido`.
5.  **Actualización:** El `stock` en `Producto` se decrementa.
6.  **Seguimiento:** Se registra el estado inicial del pedido en `SeguimientoPedido`.

---

## 📝 Documentación y Entrega

La presentación **`final_sis457_pasteleria.pdf`** deberá incluir:

1.  **Portada/Carátula:** Contextualizada con la Pastelería "Carmen".
2.  **Descripción del Negocio:** Venta remota por encargo de pastelería.
3.  **Problemática:** Necesidad de automatizar pedidos, stock y logística.
4.  **Diagrama E-R:** Diagrama completo del modelo de datos `FinalPasteleria`.
5.  **Datos de Acceso:**
    * **Repositorio GitHub:** `https://github.com/DragnelDev/sis457_pasteleria.git`.
    * **Usuario de Acceso:** `martinez`
    * **Contraseña:** `(tu_contraseña_aqui)`
6.  **Capturas de Pruebas:** Enfocadas en el **Login**, **CRUDs** y, principalmente, el flujo de **Compra/Venta** (Catálogo, Carrito y Detalle de Pedido finalizado).