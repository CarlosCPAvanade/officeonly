# 1. Resumen técnico

Solución empresarial para gestión documental colaborativa basada en:

- Angular 19 con arquitectura clásica por módulos.
- .NET 8 Web API con Clean Architecture.
- ONLYOFFICE Document Server Community como motor de edición.
- JWT para autenticación de usuarios y JWT dedicado para ONLYOFFICE.
- MySQL con Entity Framework Core.
- Docker Compose para ejecución local y VPS.
- Nginx como reverse proxy y base para HTTPS con Certbot.

# 2. Arquitectura

## Frontend

- `AppModule` como raíz.
- `CoreModule` para autenticación y seguridad.
- módulos funcionales `login`, `documents`, `editor`.
- `HttpInterceptor` para JWT.
- `AuthGuard` para rutas protegidas.

## Backend

- `Domain`: entidades y enums.
- `Application`: contratos, DTOs y servicios de negocio.
- `Infrastructure`: EF Core, repositorios, seguridad, almacenamiento.
- `Api`: controladores, middleware, bootstrap.

# 3. Estructura de carpetas

- [backend/src/Api](backend/src/Api)
- [backend/src/Application](backend/src/Application)
- [backend/src/Domain](backend/src/Domain)
- [backend/src/Infrastructure](backend/src/Infrastructure)
- [frontend/src/app](frontend/src/app)
- [docker/nginx](docker/nginx)
- [docker/certbot](docker/certbot)

# 4. Docker Compose

Archivo principal: [docker-compose.yml](docker-compose.yml)

Servicios incluidos:

- `database`
- `api`
- `angular`
- `onlyoffice`
- `nginx`
- `certbot` con perfil `prod`

# 5. Variables de entorno

Plantilla: [.env.example](.env.example)

Variables críticas:

- conexión MySQL
- JWT usuarios
- JWT ONLYOFFICE
- usuario admin inicial
- URLs públicas e internas
- dominio y correo para Let's Encrypt

# 6. Backend .NET

Entrada principal: [backend/src/Api/Program.cs](backend/src/Api/Program.cs)

Qué hace:

- carga configuración
- registra infraestructura y servicios
- configura CORS
- configura autenticación JWT
- activa controladores y Swagger
- aplica migraciones y seed inicial

# 7. Entidades

- [backend/src/Domain/Entities/User.cs](backend/src/Domain/Entities/User.cs)
- [backend/src/Domain/Entities/Role.cs](backend/src/Domain/Entities/Role.cs)
- [backend/src/Domain/Entities/Document.cs](backend/src/Domain/Entities/Document.cs)
- [backend/src/Domain/Entities/DocumentVersion.cs](backend/src/Domain/Entities/DocumentVersion.cs)
- [backend/src/Domain/Entities/DocumentPermission.cs](backend/src/Domain/Entities/DocumentPermission.cs)
- [backend/src/Domain/Entities/AuditLog.cs](backend/src/Domain/Entities/AuditLog.cs)

# 8. DbContext

Archivo: [backend/src/Infrastructure/Data/AppDbContext.cs](backend/src/Infrastructure/Data/AppDbContext.cs)

Qué hace:

- define tablas
- define relaciones
- crea índices únicos
- aplica restricciones para roles, usuarios, versiones y permisos

# 9. Migraciones

- [backend/src/Infrastructure/Data/Migrations/202605070001_InitialCreate.cs](backend/src/Infrastructure/Data/Migrations/202605070001_InitialCreate.cs)
- [backend/src/Infrastructure/Data/Migrations/AppDbContextModelSnapshot.cs](backend/src/Infrastructure/Data/Migrations/AppDbContextModelSnapshot.cs)

# 10. Repositories

- [backend/src/Infrastructure/Repositories/UserRepository.cs](backend/src/Infrastructure/Repositories/UserRepository.cs)
- [backend/src/Infrastructure/Repositories/DocumentRepository.cs](backend/src/Infrastructure/Repositories/DocumentRepository.cs)

Qué hacen:

- resuelven usuarios con rol
- obtienen documentos accesibles según permisos
- cargan versiones y permisos en una sola consulta

# 11. Services

- [backend/src/Application/Services/AuthService.cs](backend/src/Application/Services/AuthService.cs)
- [backend/src/Application/Services/DocumentService.cs](backend/src/Application/Services/DocumentService.cs)
- [backend/src/Application/Services/OnlyOfficeService.cs](backend/src/Application/Services/OnlyOfficeService.cs)
- [backend/src/Application/Services/AuditService.cs](backend/src/Application/Services/AuditService.cs)

# 12. JWT

Implementación principal: [backend/src/Infrastructure/Security/JwtTokenService.cs](backend/src/Infrastructure/Security/JwtTokenService.cs)

Incluye:

- JWT de usuario
- JWT de ONLYOFFICE
- token temporal firmado para `downloadUrl`

# 13. Controllers

- [backend/src/Api/Controllers/AuthController.cs](backend/src/Api/Controllers/AuthController.cs)
- [backend/src/Api/Controllers/DocumentsController.cs](backend/src/Api/Controllers/DocumentsController.cs)
- [backend/src/Api/Controllers/OnlyOfficeController.cs](backend/src/Api/Controllers/OnlyOfficeController.cs)

# 14. ONLYOFFICE integration

Servicio: [backend/src/Application/Services/OnlyOfficeService.cs](backend/src/Application/Services/OnlyOfficeService.cs)

Qué hace:

- valida permisos
- genera `document`, `editorConfig` y `token`
- genera `downloadUrl` temporal
- usa `callbackUrl` apuntando al backend

# 15. Callback

Endpoint: [backend/src/Api/Controllers/OnlyOfficeController.cs](backend/src/Api/Controllers/OnlyOfficeController.cs)

Lógica:

- valida JWT de callback
- descarga el archivo actualizado desde la URL enviada por ONLYOFFICE
- reemplaza el archivo actual
- crea una nueva versión persistente
- registra auditoría de edición

# 16. Angular modules

- [frontend/src/app/app.module.ts](frontend/src/app/app.module.ts)
- [frontend/src/app/features/login/login.module.ts](frontend/src/app/features/login/login.module.ts)
- [frontend/src/app/features/documents/documents.module.ts](frontend/src/app/features/documents/documents.module.ts)
- [frontend/src/app/features/editor/editor.module.ts](frontend/src/app/features/editor/editor.module.ts)

# 17. Angular services

- [frontend/src/app/core/auth/auth.service.ts](frontend/src/app/core/auth/auth.service.ts)
- [frontend/src/app/features/documents/documents.service.ts](frontend/src/app/features/documents/documents.service.ts)
- [frontend/src/app/features/editor/editor.service.ts](frontend/src/app/features/editor/editor.service.ts)

# 18. Angular components

- [frontend/src/app/features/login/login.component.ts](frontend/src/app/features/login/login.component.ts)
- [frontend/src/app/features/documents/document-list.component.ts](frontend/src/app/features/documents/document-list.component.ts)
- [frontend/src/app/features/documents/document-detail.component.ts](frontend/src/app/features/documents/document-detail.component.ts)
- [frontend/src/app/features/documents/document-upload.component.ts](frontend/src/app/features/documents/document-upload.component.ts)
- [frontend/src/app/features/editor/onlyoffice-editor.component.ts](frontend/src/app/features/editor/onlyoffice-editor.component.ts)

# 19. Guards

Guard principal: [frontend/src/app/core/auth/auth.guard.ts](frontend/src/app/core/auth/auth.guard.ts)

# 20. Interceptors

Interceptor JWT: [frontend/src/app/core/auth/auth.interceptor.ts](frontend/src/app/core/auth/auth.interceptor.ts)

# 21. Routing

- [frontend/src/app/app-routing.module.ts](frontend/src/app/app-routing.module.ts)
- [frontend/src/app/features/login/login-routing.module.ts](frontend/src/app/features/login/login-routing.module.ts)
- [frontend/src/app/features/documents/documents-routing.module.ts](frontend/src/app/features/documents/documents-routing.module.ts)
- [frontend/src/app/features/editor/editor-routing.module.ts](frontend/src/app/features/editor/editor-routing.module.ts)

# 22. Upload documentos

Frontend: [frontend/src/app/features/documents/document-upload.component.ts](frontend/src/app/features/documents/document-upload.component.ts)

Backend: [backend/src/Application/Services/DocumentService.cs](backend/src/Application/Services/DocumentService.cs)

# 23. Historial versiones

Backend: [backend/src/Application/Services/DocumentService.cs](backend/src/Application/Services/DocumentService.cs) y [backend/src/Application/Services/OnlyOfficeService.cs](backend/src/Application/Services/OnlyOfficeService.cs)

Frontend: [frontend/src/app/features/documents/document-detail.component.ts](frontend/src/app/features/documents/document-detail.component.ts)

# 24. Auditoría

Persistencia: [backend/src/Application/Services/AuditService.cs](backend/src/Application/Services/AuditService.cs)

Eventos auditados:

- login
- upload
- download
- delete
- edit
- versionado

# 25. Config localhost

URLs objetivo:

- Angular: `http://localhost:4200`
- API: `http://localhost:5000`
- ONLYOFFICE: `http://localhost:8080`

Punto clave:

- `OnlyOffice__InternalApiBaseUrl=http://host.docker.internal:5000`

# 26. Config VPS

Para Ubuntu 22.04:

1. copiar `.env.example` a `.env`
2. cambiar dominio, secretos y URLs públicas
3. sustituir [docker/nginx/default.conf](docker/nginx/default.conf) por [docker/nginx/production.conf.template](docker/nginx/production.conf.template)
4. ejecutar Certbot
5. levantar `docker compose up -d`

# 27. Nginx

- local: [docker/nginx/default.conf](docker/nginx/default.conf)
- producción: [docker/nginx/production.conf.template](docker/nginx/production.conf.template)

# 28. HTTPS

Script base: [docker/certbot/init-letsencrypt.sh](docker/certbot/init-letsencrypt.sh)

Uso esperado en VPS:

1. exportar `DOMAIN` y `LETSENCRYPT_EMAIL`
2. ejecutar el script con el contenedor de nginx disponible
3. montar certificados en nginx

# 29. Checklist pruebas

- [ ] login con admin semilla
- [ ] subir DOCX
- [ ] listar documentos
- [ ] abrir detalle
- [ ] abrir editor ONLYOFFICE
- [ ] guardar cambios
- [ ] verificar callback y nueva versión
- [ ] descargar documento
- [ ] eliminar documento
- [ ] revisar tablas `DocumentVersions` y `AuditLogs`

# 30. Comandos ejecución

## Local con Docker Compose

1. copiar `.env.example` a `.env`
2. `docker compose up --build`

## Backend local

1. restaurar solución `backend/OnlyOfficeCollab.sln`
2. ejecutar API en puerto `5000`

## Frontend local

1. instalar dependencias en `frontend`
2. ejecutar Angular en puerto `4200`

## Notas finales

Archivos especialmente críticos:

- [backend/src/Application/Services/OnlyOfficeService.cs](backend/src/Application/Services/OnlyOfficeService.cs)
- [backend/src/Application/Services/DocumentService.cs](backend/src/Application/Services/DocumentService.cs)
- [backend/src/Infrastructure/Security/JwtTokenService.cs](backend/src/Infrastructure/Security/JwtTokenService.cs)
- [frontend/src/app/features/editor/onlyoffice-editor.component.ts](frontend/src/app/features/editor/onlyoffice-editor.component.ts)
- [docker-compose.yml](docker-compose.yml)
