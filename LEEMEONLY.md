# LEEMEONLY

## Guía para instalar e integrar ONLYOFFICE en otro proyecto con .NET y Angular

Este documento describe los pasos recomendados para integrar **ONLYOFFICE Document Server** en otro proyecto que use:

- **.NET 8 Web API** o backend .NET equivalente
- **Angular 19** o frontend Angular equivalente
- autenticación JWT
- apertura y edición de documentos Office desde navegador

---

## 1. Objetivo de la integración

La integración con ONLYOFFICE debe permitir:

- abrir documentos desde Angular
- generar una configuración dinámica desde .NET
- autenticar la comunicación con JWT
- descargar el documento desde el backend
- guardar cambios mediante `callbackUrl`
- crear nuevas versiones si el proyecto lo requiere

ONLYOFFICE no debe gestionar por sí solo la lógica de negocio. El backend debe seguir controlando:

- permisos
- usuarios
- rutas de archivos
- versionado
- auditoría

---

## 2. Componentes necesarios

Para una integración funcional hacen falta cuatro piezas:

1. **ONLYOFFICE Document Server**
2. **Backend .NET** que genere la configuración del editor
3. **Frontend Angular** que cargue `api.js` y cree el editor
4. **Endpoint de callback** para guardar cambios

---

## 3. Levantar ONLYOFFICE Document Server

La forma más práctica es usar Docker.

### Ejemplo básico

```yaml
services:
  onlyoffice:
    image: onlyoffice/documentserver:latest
    container_name: onlyoffice
    restart: unless-stopped
    environment:
      JWT_ENABLED: 'true'
      JWT_SECRET: TU_SECRETO_ONLYOFFICE
      JWT_HEADER: Authorization
    ports:
      - "8080:80"
```

### Qué configurar

- `JWT_ENABLED=true`
- `JWT_SECRET` igual al que usará el backend
- puerto accesible desde navegador
- red accesible desde el backend

### Recomendación

Definir dos URLs si hay contenedores:

- **URL pública**: la usa el navegador
- **URL interna**: la usa el backend o el propio ONLYOFFICE dentro de la red Docker

Ejemplo:

- pública: `http://localhost:8080`
- interna: `http://onlyoffice`

---

## 4. Requisitos del backend .NET

El backend debe implementar estos bloques:

- endpoint para obtener configuración del editor
- endpoint para descargar el documento
- endpoint para recibir callback de ONLYOFFICE
- generación de JWT específico para ONLYOFFICE
- validación de permisos del usuario

### Endpoints mínimos recomendados

- `GET /api/documents/{id}/config`
- `GET /api/documents/{id}/download`
- `POST /api/onlyoffice/callback/{id}`

---

## 5. Configuración necesaria en .NET

Definir configuración tipo:

```json
{
  "OnlyOffice": {
    "DocumentServerUrl": "http://localhost:8080",
    "InternalDocumentServerUrl": "http://onlyoffice",
    "JwtSecret": "TU_SECRETO_ONLYOFFICE",
    "InternalApiBaseUrl": "http://host.docker.internal:5000",
    "PublicApiBaseUrl": "http://localhost:5000",
    "UrlExpirationMinutes": 20
  }
}
```

### Significado

- `DocumentServerUrl`: URL pública del servidor ONLYOFFICE
- `InternalDocumentServerUrl`: URL interna para red entre contenedores
- `JwtSecret`: secreto compartido con ONLYOFFICE
- `InternalApiBaseUrl`: URL que ONLYOFFICE usará para llamar al backend
- `PublicApiBaseUrl`: URL pública del backend
- `UrlExpirationMinutes`: validez de URLs o tokens temporales

---

## 6. Generar la configuración del editor desde .NET

Cuando Angular quiera abrir un documento, el backend debe devolver un objeto de configuración con:

- tipo de documento
- clave única de versión
- nombre del archivo
- URL de descarga
- permisos de edición
- `callbackUrl`
- usuario actual
- token JWT ONLYOFFICE

### Campos típicos

```json
{
  "documentType": "word",
  "type": "desktop",
  "document": {
    "fileType": "docx",
    "key": "documento-v3",
    "title": "contrato.docx",
    "url": "http://backend/api/documents/1/download?accessToken=...",
    "permissions": {
      "edit": true,
      "download": true,
      "comment": true,
      "review": true,
      "print": true
    }
  },
  "editorConfig": {
    "callbackUrl": "http://backend/api/onlyoffice/callback/1",
    "mode": "edit",
    "lang": "es",
    "user": {
      "id": "123",
      "name": "admin"
    }
  },
  "token": "JWT_FIRMADO"
}
```

### Importante

- `key` debe cambiar cuando el documento cambie de versión
- `url` debe ser accesible por ONLYOFFICE
- `callbackUrl` debe ser accesible por ONLYOFFICE
- el token debe firmarse con el mismo secreto configurado en el contenedor

---

## 7. Implementar descarga segura del documento

ONLYOFFICE descargará el archivo desde una URL del backend.

Se recomienda:

- no exponer rutas físicas reales
- usar endpoint controlado por permisos
- usar token temporal firmado en la URL

### Ejemplo de flujo

1. Angular solicita la config.
2. El backend genera `downloadUrl` con token temporal.
3. ONLYOFFICE descarga el archivo desde esa URL.
4. El backend valida el token y responde con el archivo.

---

## 8. Implementar el callback de ONLYOFFICE

ONLYOFFICE enviará notificaciones al backend cuando haya cambios o guardados.

El endpoint debe:

1. validar el token recibido
2. leer el estado (`status`)
3. descargar el archivo actualizado desde la URL que devuelve ONLYOFFICE
4. reemplazar el archivo actual o generar nueva versión
5. devolver:

```json
{ "error": 0 }
```

### Estados más habituales

- `2`: documento guardado y listo para persistir
- `6`: `force save`

### Recomendaciones

- validar siempre JWT del callback
- si el backend corre en contenedor, usar la URL interna de ONLYOFFICE para descargar el binario actualizado
- registrar auditoría si el proyecto lo requiere

---

## 9. Cargar ONLYOFFICE en Angular

En Angular no se instala un paquete complejo; normalmente se carga el script `api.js` del servidor ONLYOFFICE.

### Script necesario

```text
http://localhost:8080/web-apps/apps/api/documents/api.js
```

O usando la URL configurada del servidor.

### Flujo en Angular

1. Obtener el `id` del documento.
2. Pedir al backend `/config`.
3. Cargar `api.js` dinámicamente.
4. Crear `new DocsAPI.DocEditor(...)`.

### Requisitos en el componente Angular

- contenedor HTML para montar el editor
- servicio para pedir la configuración
- destrucción correcta del editor al salir
- control de estados de carga y error

---

## 10. Estructura recomendada en Angular

### Servicio de editor

Debe encargarse de:

- pedir la configuración al backend
- cargar `api.js`
- crear el editor
- destruir la instancia cuando cambie la vista

### Componente de editor

Debe encargarse de:

- leer el `documentId` desde la ruta
- invocar el servicio
- montar el editor en un `div`
- mostrar errores de integración

### HTML recomendado

- un contenedor principal
- un `div` con alto suficiente
- evitar destruir el host mientras el editor se carga

---

## 11. Configuración del frontend

En Angular definir variables tipo:

```ts
export const environment = {
  production: false,
  apiBaseUrl: 'http://localhost:5000',
  onlyOfficeUrl: 'http://localhost:8080'
};
```

### Debe coincidir con:

- la URL pública del backend
- la URL pública del servidor ONLYOFFICE

---

## 12. Permisos y seguridad

Antes de devolver la configuración del editor, el backend debe validar:

- que el usuario está autenticado
- que tiene permiso sobre el documento
- si debe abrirse en modo `edit` o `view`

### Recomendación de permisos

- `admin`: puede editar todo
- `editor`: puede editar según permiso del documento
- `reader`: solo visualización

---

## 13. CORS y red

Comprobar siempre:

- Angular puede llamar al backend
- Angular puede cargar `api.js` desde ONLYOFFICE
- ONLYOFFICE puede llamar al callback del backend
- ONLYOFFICE puede descargar el documento desde el backend

### Error común

No usar `localhost` de forma incorrecta dentro de contenedores.

Recordatorio:

- `localhost` en el navegador no es lo mismo que `localhost` dentro del contenedor del backend
- `localhost` dentro del backend no es lo mismo que el contenedor de ONLYOFFICE

---

## 14. Validación mínima paso a paso

### Fase 1 - Servidor ONLYOFFICE

- responde en navegador
- carga `api.js`
- JWT activado correctamente

### Fase 2 - Backend

- devuelve config válida
- descarga documento correctamente
- callback responde `{"error":0}`

### Fase 3 - Frontend Angular

- carga el componente editor
- monta `DocsAPI.DocEditor`
- abre `DOCX`, `XLSX` y `PPTX`

### Fase 4 - Guardado real

- editar documento
- guardar desde ONLYOFFICE
- recibir callback
- actualizar archivo
- crear nueva versión si aplica

---

## 15. Problemas frecuentes

### 1. El editor no carga

Posibles causas:

- `api.js` no accesible
- URL de ONLYOFFICE incorrecta
- CORS
- contenedor HTML no existe aún

### 2. El documento abre pero no guarda

Posibles causas:

- `callbackUrl` no accesible
- JWT del callback incorrecto
- backend no puede descargar el archivo actualizado
- respuesta del callback distinta de `{"error":0}`

### 3. Error de red al abrir el editor

Posibles causas:

- `DocumentServerUrl` incorrecta
- mezcla de URL pública e interna
- navegador usando una URL que solo existe dentro de Docker

### 4. Error al descargar el archivo desde callback

Posibles causas:

- el backend usa `localhost:8080` dentro de contenedor
- debe usar `InternalDocumentServerUrl`

---

## 16. Archivos que conviene implementar en el nuevo proyecto

### Backend .NET

- `OnlyOfficeOptions`
- `IOnlyOfficeService`
- `OnlyOfficeService`
- `JwtTokenService`
- `DocumentsController`
- `OnlyOfficeController`
- servicio de almacenamiento local o equivalente

### Angular

- `editor.service.ts`
- `editor.component.ts`
- `editor.component.html`
- `environment.ts`
- `documents.service.ts`

### Archivos mínimos para reutilizar el wrapper oficial

Si quieres llevarte solo la integración basada en `@onlyoffice/document-editor-angular@6.5.1`, copia como mínimo:

- `frontend/src/app/features/editor/editor-angular.component.ts`
- `frontend/src/app/features/editor/editor-angular.component.html`
- `frontend/src/app/features/editor/editor-angular.component.css`
- `frontend/src/app/features/editor/editor.service.ts`
- `frontend/src/app/features/editor/editor-routing.module.ts`
- `frontend/src/app/features/editor/editor.module.ts`
- `frontend/src/app/shared/models/document.models.ts`
- `frontend/src/environments/environment.ts`
- `frontend/src/environments/environment.prod.ts`

Y además reproduce estas piezas de soporte:

- dependencia `@onlyoffice/document-editor-angular@6.5.1` en `package.json`
- `allowedCommonJsDependencies: ["lodash"]` en `angular.json`
- endpoint backend `GET /api/documents/{id}/config`
- endpoint backend `POST /api/onlyoffice/callback/{id}`
- endpoint backend `GET /api/documents/{id}/download`

Con ese conjunto ya puedes abrir el editor wrapper en otro proyecto Angular modular sin copiar la implementación manual con `api.js`.

---

## 17. Resultado esperado

Al terminar la instalación e integración, el nuevo proyecto debe poder:

- abrir documentos Office en navegador
- diferenciar modo edición y visualización
- descargar el archivo desde backend de forma segura
- guardar cambios desde ONLYOFFICE mediante callback
- mantener trazabilidad y versionado si el proyecto lo implementa

---

## 18. Recomendación final

La mejor estrategia es integrar ONLYOFFICE en este orden:

1. levantar el servidor ONLYOFFICE
2. implementar la config del editor en .NET
3. implementar la descarga segura del archivo
4. implementar el callback
5. montar el editor en Angular
6. validar apertura
7. validar guardado y versionado

Si el proyecto usa contenedores, revisar cuidadosamente las **URLs públicas e internas**, porque es el punto que más errores produce en integraciones con ONLYOFFICE.
