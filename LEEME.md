# LEEME

## Pasos para trasladar esta aplicación a otra solución con .NET 8, Angular 19, SQL Server y un servidor de pruebas de ONLYOFFICE

## Objetivo

Usar esta aplicación como base funcional para construir o portar una solución equivalente que mantenga:

- Frontend en Angular 19
- Backend en .NET 8 Web API
- Integración con ONLYOFFICE
- Autenticación JWT
- Gestión documental con permisos, versionado y auditoría
- Base de datos SQL Server en lugar de MySQL
- Servidor de pruebas de ONLYOFFICE para entornos de test

## 1. Analizar el alcance de la migración

Antes de mover código, identificar qué partes se van a conservar exactamente:

- modelo de datos
- estructura modular del frontend
- endpoints de autenticación y documentos
- integración con ONLYOFFICE
- lógica de callback y versionado
- auditoría y seguridad

Se recomienda mantener la misma separación por capas:

- `Domain`
- `Application`
- `Infrastructure`
- `Api`

Y en frontend:

- `core`
- `shared`
- `features/login`
- `features/documents`
- `features/editor`

## 2. Crear la nueva solución backend en .NET 8

Crear una nueva solución .NET 8 con proyectos separados:

- `Api`
- `Application`
- `Domain`
- `Infrastructure`

Pasos recomendados:

1. Crear solución nueva.
2. Crear librerías de clases para `Domain`, `Application` e `Infrastructure`.
3. Crear proyecto Web API para `Api`.
4. Referenciar proyectos:
   - `Api` -> `Application`, `Infrastructure`
   - `Infrastructure` -> `Application`, `Domain`
   - `Application` -> `Domain`
5. Copiar progresivamente:
   - entidades
   - DTOs
   - interfaces
   - servicios
   - middleware
   - controladores
   - configuración JWT

## 3. Adaptar la base de datos a SQL Server

La versión actual usa MySQL. Para moverla a SQL Server hay que cambiar proveedor EF Core y conexión.

### 3.1. Sustituir paquetes NuGet

Eliminar dependencia MySQL:

- `Pomelo.EntityFrameworkCore.MySql`

Agregar paquetes SQL Server:

- `Microsoft.EntityFrameworkCore.SqlServer`
- `Microsoft.EntityFrameworkCore.Design`
- `Microsoft.EntityFrameworkCore.Tools`

### 3.2. Cambiar el registro del DbContext

En la configuración de `Infrastructure`, sustituir el proveedor MySQL por SQL Server.

Ejemplo conceptual:

- antes: `UseMySql(...)`
- después: `UseSqlServer(...)`

### 3.3. Revisar tipos de datos

Verificar en `AppDbContext` y migraciones:

- `longtext`
- tamaños `varchar`
- índices únicos
- restricciones y `delete behaviors`

Algunos tipos específicos de MySQL deberán eliminarse o convertirse a tipos compatibles con SQL Server.

### 3.4. Generar nuevas migraciones

No reutilizar directamente las migraciones de MySQL para SQL Server.

Pasos:

1. Eliminar migraciones antiguas si la nueva solución parte desde cero.
2. Crear migración inicial nueva para SQL Server.
3. Aplicarla contra una base SQL Server limpia.

### 3.5. Cadena de conexión SQL Server

Usar una cadena similar a:

```text
Server=localhost,1433;Database=OnlyOfficeCollab;User Id=sa;Password=TuPasswordSegura;TrustServerCertificate=True;
```

Si el SQL Server está en Docker:

- exponer puerto `1433`
- configurar `SA_PASSWORD`
- aceptar `EULA`

## 4. Migrar la configuración del backend

Revisar estos puntos en `appsettings` y variables de entorno:

- `ConnectionStrings__DefaultConnection`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Jwt__Secret`
- `OnlyOffice__JwtSecret`
- `OnlyOffice__DocumentServerUrl`
- `OnlyOffice__InternalDocumentServerUrl`
- `OnlyOffice__InternalApiBaseUrl`
- `OnlyOffice__PublicApiBaseUrl`
- `Storage__RootPath`
- `Seed__AdminUserName`
- `Seed__AdminEmail`
- `Seed__AdminPassword`

Si el entorno nuevo usa dominio de pruebas, actualizar todas las URLs públicas e internas.

## 5. Preparar el servicio de almacenamiento

La solución guarda archivos físicamente y mantiene rutas en base de datos.

Hay que trasladar:

- servicio de almacenamiento local
- estructura de carpetas para documentos actuales
- estructura de carpetas para versiones
- reemplazo de archivo actual al recibir callback
- copia de versiones históricas

Comprobar que la nueva aplicación tenga permisos de escritura en:

- carpeta de datos
- carpeta de versiones
- carpeta temporal si aplica

## 6. Portar la integración con ONLYOFFICE

La integración no depende del editor visual solamente; depende sobre todo del backend.

Piezas críticas a trasladar:

- generación de configuración dinámica del editor
- JWT específico para ONLYOFFICE
- URL temporal firmada de descarga
- callback funcional
- resolución de URL pública e interna
- detección de permisos de lectura y edición

### 6.1. Configuración del servidor de pruebas de ONLYOFFICE

Definir dos URLs si hay contenedores o redes separadas:

- URL pública para el navegador
- URL interna para comunicación entre servicios

Ejemplo:

- Pública: `http://onlyoffice-test.midominio.local`
- Interna: `http://onlyoffice`

### 6.2. Validar JWT de ONLYOFFICE

El secreto configurado en ONLYOFFICE debe coincidir exactamente con:

- `OnlyOffice__JwtSecret`

### 6.3. Probar el callback

Casos mínimos a validar:

- apertura de `DOCX`
- apertura de `XLSX`
- apertura de `PPTX`
- edición y guardado
- `force save`
- creación de nueva versión
- descarga del archivo modificado

## 7. Crear el nuevo frontend en Angular 19

Crear una aplicación Angular 19 manteniendo arquitectura clásica por módulos.

Módulos recomendados:

- `app`
- `core`
- `shared`
- `login`
- `documents`
- `editor`

Copiar o reimplementar:

- modelos compartidos
- `auth service`
- `auth guard`
- `auth interceptor`
- pantallas de login
- listado documental
- detalle documental
- carga de documentos
- integración del editor
- `routing` modular

Mantener:

- `Reactive Forms`
- `HttpClient`
- `Observables`
- componentes no `standalone`

## 8. Adaptar los environments del frontend

Actualizar:

- `apiBaseUrl`
- `onlyOfficeUrl`

Asegurar que el frontend apunte al backend nuevo y al servidor de pruebas de ONLYOFFICE.

Comprobar también:

- `CORS` en backend
- puertos publicados
- rutas protegidas
- carga de `api.js` del editor

## 9. Revisar autenticación y permisos

Migrar y validar:

- login JWT
- almacenamiento de sesión
- roles de usuario
- permisos por documento
- acceso de `admin`
- acceso de `editor`
- acceso de `reader`

Pruebas mínimas:

- usuario sin permisos no puede abrir documento
- usuario con lectura abre en modo `view`
- usuario con edición abre en modo `edit`

## 10. Adaptar Docker o Podman del nuevo proyecto

Si la nueva solución va a usar contenedores, crear o ajustar:

- `Dockerfile` del backend
- `Dockerfile` del frontend
- `docker-compose.yml` o equivalente
- servicio SQL Server
- servicio ONLYOFFICE de pruebas
- proxy `Nginx` si aplica

Para SQL Server en contenedor, usar variables típicas:

- `ACCEPT_EULA=Y`
- `SA_PASSWORD=clave-segura`

Revisar dependencias entre servicios:

- `api` depende de `sqlserver`
- `frontend` depende de `api`
- `onlyoffice` debe poder llamar al callback del backend

## 11. Validación funcional por fases

Se recomienda validar en este orden:

### Fase 1 - Backend base

- la API arranca
- Swagger responde
- SQL Server conecta
- migraciones aplican
- seed de admin funciona

### Fase 2 - Seguridad

- login devuelve JWT
- endpoints protegidos responden correctamente

### Fase 3 - Documentos

- subir documento
- listar documento
- descargar documento
- eliminar documento

### Fase 4 - ONLYOFFICE

- obtener config del editor
- cargar editor en navegador
- abrir archivo desde ONLYOFFICE
- guardar cambios por callback
- generar versión nueva

### Fase 5 - Auditoría

- registrar login
- registrar subida
- registrar descarga
- registrar edición
- registrar eliminación

## 12. Archivos clave a tomar como referencia

### Backend

- `backend/src/Api/Program.cs`
- `backend/src/Api/Controllers/AuthController.cs`
- `backend/src/Api/Controllers/DocumentsController.cs`
- `backend/src/Api/Controllers/OnlyOfficeController.cs`
- `backend/src/Application/Services/AuthService.cs`
- `backend/src/Application/Services/DocumentService.cs`
- `backend/src/Application/Services/OnlyOfficeService.cs`
- `backend/src/Infrastructure/Data/AppDbContext.cs`
- `backend/src/Infrastructure/Security/JwtTokenService.cs`
- `backend/src/Infrastructure/Storage/LocalFileStorageService.cs`

### Frontend

- `frontend/src/app/core/auth/auth.service.ts`
- `frontend/src/app/core/auth/auth.guard.ts`
- `frontend/src/app/core/auth/auth.interceptor.ts`
- `frontend/src/app/features/login/`
- `frontend/src/app/features/documents/`
- `frontend/src/app/features/editor/`

### Infraestructura

- `docker-compose.yml`
- `backend/Dockerfile`
- `frontend/Dockerfile`
- `.env.example`

## 13. Riesgos comunes en la migración

- usar `localhost` donde debería usarse una URL interna entre contenedores
- mantener migraciones MySQL en un proyecto SQL Server
- no alinear el secreto JWT de ONLYOFFICE entre backend y Document Server
- no revisar CORS al mover frontend y backend a nuevos dominios
- no configurar correctamente la descarga del archivo desde ONLYOFFICE
- no conservar el flujo de versionado al guardar cambios
- no validar permisos por documento antes de abrir el editor

## 14. Recomendación de estrategia

La forma más segura de trasladar la aplicación es:

1. Crear la nueva solución vacía.
2. Migrar primero `Domain` y `Application`.
3. Adaptar `Infrastructure` a SQL Server.
4. Levantar backend y validar login.
5. Migrar frontend Angular 19.
6. Integrar ONLYOFFICE al final.
7. Validar callback y versionado antes de pasar a pruebas funcionales completas.

## 15. Resultado esperado

Al finalizar, la nueva solución debe poder:

- autenticar usuarios con JWT
- trabajar con SQL Server
- gestionar documentos Office
- abrir documentos en un servidor de pruebas de ONLYOFFICE
- guardar cambios reales por callback
- generar nuevas versiones
- mantener auditoría y permisos

## 16. Nota final

Este proyecto ya contiene la lógica funcional principal. La migración no debe hacerse copiando todo sin revisar, sino reutilizando la arquitectura, los servicios y la integración ONLYOFFICE, adaptando especialmente:

- proveedor EF Core
- migraciones
- cadenas de conexión
- URLs públicas e internas
- infraestructura de despliegue
