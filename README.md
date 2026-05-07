# ONLYOFFICE Collaborative Document Management

Proyecto empresarial full stack con Angular 19 modular, .NET 8 Web API, ONLYOFFICE Document Server Community, JWT, MySQL, Docker Compose, Nginx y despliegue para Ubuntu.

## Guía principal

La documentación funcional y técnica completa está en [docs/IMPLEMENTACION.md](docs/IMPLEMENTACION.md).

## Estructura principal

- [frontend](frontend)
- [backend](backend)
- [docker-compose.yml](docker-compose.yml)
- [docker/nginx/default.conf](docker/nginx/default.conf)
- [.env.example](.env.example)

## Credenciales iniciales

Las credenciales iniciales del administrador se configuran mediante variables de entorno:

- `SEED_ADMIN_USER`
- `SEED_ADMIN_EMAIL`
- `SEED_ADMIN_PASSWORD`

## Flujo funcional

1. Login JWT.
2. Listado documental por permisos.
3. Subida de documentos DOCX, XLSX y PPTX.
4. Apertura del editor ONLYOFFICE con config dinámica.
5. Descarga segura vía URL temporal firmada.
6. Callback funcional que reemplaza el archivo actual y genera nueva versión.
7. Auditoría persistente de login, carga, descarga, edición y eliminación.
