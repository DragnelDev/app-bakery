# Cambios de la refactorización (Clean Code / SOLID)

## Puesta en marcha
1. Base de datos nueva: ejecuta `ddl_dml_final_pasteleria.sql`.
   Base existente: ejecuta `scripts/01_metodos_pago.sql` y `scripts/02_pedido_direccion.sql` (son idempotentes).
2. Conexión: copia `WebPasteleria/appsettings.Development.example.json` a `appsettings.Development.json`
   y ajústala. Ese archivo está en `.gitignore`. Si ya estaba versionado:
   `git rm --cached WebPasteleria/appsettings.Development.json`.
   En producción usa `dotnet user-secrets` o la variable `ConnectionStrings__DefaultConnection`.
3. Las contraseñas existentes siguen funcionando: en el primer login correcto se migran a PBKDF2.
4. Correo (formulario de contacto): completa la sección `Correo` de `appsettings.json`. Sin ella el
   formulario avisa que no puede recibir mensajes.
5. `dotnet test` ejecuta las pruebas de la lógica pura.

## Arquitectura
- `Domain/` constantes y reglas puras (estados, workflow de pedidos, roles).
- `Services/` lógica de negocio detrás de interfaces (DIP); los controladores solo manejan HTTP (SRP).
- `ViewModels/` con DataAnnotations; las entidades ya no se enlazan directamente desde formularios.
- `Extensions/` composición de dependencias y rate limiting.
- `Configuration/` opciones tipadas (envío, pedidos, seguridad, correo).

## Seguridad
- Panel de administración protegido por rol (Usuarios y Empleados: solo Administrador).
- PBKDF2-SHA256 (600 000 iteraciones) con sal; bloqueo tras 5 intentos fallidos.
- Precios, totales y envío se calculan en el servidor; stock descontado de forma atómica.
- Rate limiting: consultas públicas de pedidos (20/min por IP) y login (10 / 5 min por IP).
  Detrás de un proxy inverso configura `ForwardedHeaders` para que se use la IP real.
- Antiforgery validado en todos los POST, incluidos los AJAX.

## Pendiente / decisiones para ti
- El carrito sigue en `localStorage` hasta el checkout (las tablas `Carrito`/`CarritoDetalle` no se usan).
- Si quieres un portal de clientes (Mi Cuenta) hay que agregar contraseña a `Cliente`; la versión anterior
  permitía entrar solo con el email y se eliminó.
- El correo al cliente por cada cambio de estado ya funciona, pero solo se envía si completas la sección `Correo`.
