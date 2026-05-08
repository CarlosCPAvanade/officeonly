# ONLYOFFICE Collaborative Document Management

Plataforma empresarial full stack para gestión documental y edición colaborativa en tiempo real con Angular 19, .NET 8 Web API y ONLYOFFICE Document Server Community.

El sistema permite autenticación segura con JWT, gestión de documentos Office, apertura de archivos en editor web, control de acceso por roles y permisos, versionado automático y auditoría persistente.

## Objetivo del proyecto

Proveer una base funcional, mantenible y escalable para soluciones documentales empresariales donde:

- el frontend gestiona autenticación, navegación y experiencia de usuario
- el backend controla seguridad, documentos, permisos, versiones y auditoría
- ONLYOFFICE actúa como motor de edición documental
- MySQL persiste usuarios, roles, documentos, historial y trazabilidad

## Stack tecnológico

### Frontend

- Angular 19
- TypeScript
- Angular Router
- RxJS
- Reactive Forms
- HttpClient
- arquitectura clásica basada en módulos

### Backend

- .NET 8 Web API
- Clean Architecture
- Entity Framework Core
- JWT Bearer Authentication
- Swagger / OpenAPI

### Motor de edición documental

- ONLYOFFICE Document Server Community

### Base de datos

- MySQL 8

### Infraestructura y despliegue

- Docker / Podman
- Docker Compose / podman-compose
- Nginx
- Certbot
- Ubuntu VPS como objetivo de despliegue productivo

## Software necesario

Para desarrollo o ejecución local se recomienda disponer de:

- Node.js 20 o superior
- Angular CLI 19
- .NET SDK 8
- Docker Desktop o Podman
- Git
- navegador moderno compatible con ONLYOFFICE

### Opcional

- Visual Studio Code
- Postman o herramienta equivalente para probar la API

## Librerías y dependencias principales

### Frontend

- `@angular/core`
- `@angular/common`
- `@angular/router`
- `@angular/forms`
- `@angular/common/http`
- `rxjs`

### Backend

- `Microsoft.AspNetCore.Authentication.JwtBearer`
- `Microsoft.EntityFrameworkCore`
- `Pomelo.EntityFrameworkCore.MySql`
- `Swashbuckle.AspNetCore`
- `BCrypt.Net-Next`

### Integración ONLYOFFICE

- carga dinámica de `api.js`
- configuración de editor generada por backend
- JWT específico para ONLYOFFICE
- callback funcional para persistencia real de cambios

## Características principales

### Seguridad

- autenticación con JWT
- protección de rutas en Angular mediante `AuthGuard`
- interceptor HTTP para adjuntar token
- validación de permisos desde backend
- validación de callback y tokens temporales firmados

### Gestión documental

- listado de documentos accesibles según permisos
- subida de archivos Office
- descarga segura de documentos
- eliminación lógica de documentos
- consulta de detalle e historial de versiones

### Edición colaborativa

- integración web con ONLYOFFICE
- generación dinámica de `document`, `editorConfig` y `token`
- callback para guardar cambios reales
- soporte para edición y visualización según permisos

### Versionado y auditoría

- creación automática de nuevas versiones
- registro de usuario que realiza el cambio
- historial por documento
- auditoría persistente de login, carga, descarga, edición y eliminación

## Qué puede hacer el sistema

La solución permite:

- autenticar usuarios con sesión basada en JWT
- administrar el acceso por rol y permisos documentales
- almacenar documentos Office en el servidor
- abrir documentos en ONLYOFFICE desde el navegador
- recibir y persistir cambios desde el callback del editor
- mantener historial de versiones por documento
- registrar trazabilidad completa de acciones de usuario
- ejecutarse en localhost, contenedores y VPS Ubuntu

## Formatos soportados

- `DOCX`
- `XLSX`
- `PPTX`

## Arquitectura

### Frontend

Módulos principales:

- `AppModule`
- `CoreModule`
- `SharedModule`
- módulo `login`
- módulo `documents`
- módulo `editor`

Ubicación principal:

- [frontend/src/app](frontend/src/app)

### Backend

Capas principales:

- `Domain`: entidades y enums
- `Application`: contratos, DTOs y servicios
- `Infrastructure`: persistencia, repositorios, seguridad y almacenamiento
- `Api`: controladores, middleware y bootstrap

Ubicación principal:

- [backend/src/Api](backend/src/Api)
- [backend/src/Application](backend/src/Application)
- [backend/src/Domain](backend/src/Domain)
- [backend/src/Infrastructure](backend/src/Infrastructure)

## Estructura principal del proyecto

- [frontend](frontend)
- [backend](backend)
- [docker-compose.yml](docker-compose.yml)
- [docker/nginx/default.conf](docker/nginx/default.conf)
- [.env.example](.env.example)
- [docs/IMPLEMENTACION.md](docs/IMPLEMENTACION.md)

## Endpoints principales

### Autenticación

- `POST /api/auth/login`

### Documentos

- `GET /api/documents`
- `POST /api/documents/upload`
- `GET /api/documents/{id}`
- `DELETE /api/documents/{id}`
- `GET /api/documents/{id}/download`
- `GET /api/documents/{id}/versions`
- `GET /api/documents/{id}/config`

### ONLYOFFICE

- `POST /api/onlyoffice/callback/{id}`

## Flujo funcional

1. El usuario inicia sesión con JWT.
2. Se obtiene el listado documental en función de sus permisos.
3. El usuario sube un documento `DOCX`, `XLSX` o `PPTX`.
4. El frontend solicita la configuración dinámica del editor.
5. ONLYOFFICE descarga el archivo mediante una URL temporal firmada.
6. El usuario edita el documento en el navegador.
7. El callback funcional reemplaza el archivo actual y genera una nueva versión.
8. Se registra auditoría persistente de la operación realizada.

## Puertos por defecto en desarrollo

- Angular: `http://localhost:4200`
- API: `http://localhost:5000`
- ONLYOFFICE: `http://localhost:8080`

## Variables de entorno relevantes

Las credenciales iniciales del administrador y la configuración del entorno se definen mediante variables de entorno, tomando como base [ .env.example](.env.example):

- `SEED_ADMIN_USER`
- `SEED_ADMIN_EMAIL`
- `SEED_ADMIN_PASSWORD`
- `JWT_SECRET`
- `ONLYOFFICE_JWT_SECRET`
- `API_PUBLIC_BASE_URL`
- `API_INTERNAL_BASE_URL`
- `ONLYOFFICE_PUBLIC_URL`
- `ONLYOFFICE_INTERNAL_URL`

## Credenciales iniciales

Si se usan los valores por defecto del entorno de desarrollo:

- usuario: `admin`
- password: `ChangeThisAdminPassword!`

## Ejecución rápida

1. Copiar [.env.example](.env.example) a `.env`.
2. Levantar el stack con `docker compose up --build -d` o alternativa equivalente con Podman.
3. Acceder a Angular, API y ONLYOFFICE en sus puertos por defecto.

## Detalles importantes de implementación

- el backend genera JWT de usuario y JWT específico para ONLYOFFICE
- el callback descarga el archivo actualizado y crea una nueva versión persistente
- la descarga de documentos usa tokens temporales firmados
- la solución está preparada para reverse proxy con Nginx y HTTPS con Certbot
- la documentación técnica ampliada está en [docs/IMPLEMENTACION.md](docs/IMPLEMENTACION.md)
