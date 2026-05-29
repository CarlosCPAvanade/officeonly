# Token JWT en OnlyOfficeController

Este documento explica como se genera el token JWT en `OnlyOfficeController`, que datos contiene y como se incorpora a la configuracion que consume ONLYOFFICE.

## Punto de entrada

La construccion de la configuracion del editor ocurre en el endpoint:

- `GET /api/v1/onlyoffice/config/{documentId}`

Ese endpoint esta implementado en `GetConfig(Guid documentId)` dentro de `backend/Controllers/OnlyOfficeController.cs`.

El flujo es este:

1. Lee `OnlyOffice:BackendPublicBaseUrl` desde configuracion.
2. Localiza o inicializa el documento en el catalogo.
3. Construye `fileUrl` y `callbackUrl`.
4. Monta un objeto `OnlyOfficeEditorConfig` con `Document`, `DocumentType` y `EditorConfig`.
5. Si JWT esta habilitado, asigna `config.Token = GenerateToken(config)`.
6. Devuelve `Ok(config)` al frontend.

## Donde se decide si hay token

La decision se toma en el metodo privado `IsJwtEnabled()`:

```csharp
private bool IsJwtEnabled()
{
    return bool.TryParse(_configuration["OnlyOffice:JwtEnabled"], out var enabled) && enabled;
}
```

Si `OnlyOffice:JwtEnabled` vale `true`, el backend firma el token y lo mete dentro de la respuesta JSON.

## Configuracion necesaria

En `backend/appsettings.Development.json` aparecen las claves implicadas:

```json
"OnlyOffice": {
  "FrontendBaseUrl": "http://localhost:4200",
  "BackendPublicBaseUrl": "http://host.docker.internal:5033",
  "DocumentServerInternalUrl": "http://localhost:8083",
  "JwtEnabled": true,
  "JwtSecret": "onlyoffice-secret-key"
}
```

Las dos claves importantes para el token son:

- `JwtEnabled`: activa o desactiva la firma.
- `JwtSecret`: secreto compartido entre backend y ONLYOFFICE para validar la firma HS256.

## Como se crea el token

La firma ocurre en el metodo `GenerateToken(OnlyOfficeEditorConfig config)`.

### 1. Lee el secreto

```csharp
var secret = _configuration["OnlyOffice:JwtSecret"];
if (string.IsNullOrWhiteSpace(secret))
{
    throw new InvalidOperationException("Falta la configuración OnlyOffice:JwtSecret.");
}
```

Sin secreto no se puede firmar el JWT.

### 2. Construye el payload

El payload no es generico: se arma a partir del mismo objeto `config` que luego se devolvera al frontend.

Incluye estas ramas:

- `document`
- `documentType`
- `editorConfig`

Dentro de `document` se meten:

- `fileType`
- `key`
- `title`
- `url`
- `permissions`

Dentro de `editorConfig` se meten:

- `callbackUrl`
- `createUrl`
- `mode`
- `customization`
- `user`

En otras palabras: el token firma exactamente la informacion critica que ONLYOFFICE usara para abrir y editar el documento.

### 3. Construye la cabecera JWT

El header se define asi:

```csharp
var header = new Dictionary<string, object>
{
    { "alg", "HS256" },
    { "typ", "JWT" }
};
```

Esto indica:

- `alg = HS256`: firma HMAC SHA-256.
- `typ = JWT`: formato JSON Web Token.

### 4. Serializa y convierte a Base64 URL-safe

Primero serializa `header` y `payload` a bytes UTF-8:

```csharp
var headerEncoded = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(header));
var payloadEncoded = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(payload));
```

Despues construye la entrada de firma:

```csharp
var signatureInput = $"{headerEncoded}.{payloadEncoded}";
```

Ese formato es el normal de JWT.

### 5. Firma con HMAC SHA-256

```csharp
using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
var signature = hmac.ComputeHash(Encoding.UTF8.GetBytes(signatureInput));
```

Aqui la firma depende de dos cosas:

- el contenido serializado de `header.payload`
- el secreto `JwtSecret`

### 6. Devuelve el token final

```csharp
return $"{signatureInput}.{Base64UrlEncode(signature)}";
```

El token resultante tiene el formato clasico:

```text
header.payload.signature
```

## Que hace Base64UrlEncode

El metodo auxiliar es este:

```csharp
private static string Base64UrlEncode(byte[] data)
{
    return Convert.ToBase64String(data)
        .TrimEnd('=')
        .Replace('+', '-')
        .Replace('/', '_');
}
```

Eso adapta la salida Base64 normal al formato URL-safe que exige JWT:

- elimina `=` al final
- cambia `+` por `-`
- cambia `/` por `_`

## Como se implementa en config

Una vez creado el objeto `config`, el controlador hace esto:

```csharp
if (IsJwtEnabled())
{
    config.Token = GenerateToken(config);
}
```

La clase `OnlyOfficeEditorConfig` tiene esta propiedad:

```csharp
public sealed class OnlyOfficeEditorConfig
{
    public OnlyOfficeDocumentConfig Document { get; set; } = new();
    public string DocumentType { get; set; } = "word";
    public OnlyOfficeEditorSettings EditorConfig { get; set; } = new();
    public string? Token { get; set; }
}
```

Por tanto, la respuesta JSON que recibe el frontend contiene tanto la configuracion como el token en el mismo objeto.

Un esquema simplificado de la respuesta seria:

```json
{
  "document": {
    "fileType": "docx",
    "key": "doc-...",
    "title": "archivo.docx",
    "url": "http://.../api/v1/onlyoffice/files/...",
    "permissions": {
      "copy": true,
      "download": true,
      "edit": true,
      "print": true,
      "review": true
    }
  },
  "documentType": "word",
  "editorConfig": {
    "callbackUrl": "http://.../api/v1/onlyoffice/callback/...",
    "createUrl": "/onlyoffice",
    "mode": "edit",
    "customization": {
      "autosave": true,
      "forcesave": true
    },
    "user": {
      "id": "1",
      "name": "Administrador"
    }
  },
  "token": "header.payload.signature"
}
```

## Relacion entre config y token

La idea importante es esta:

- `config` contiene los datos operativos.
- `token` contiene una firma JWT de esos datos.

ONLYOFFICE recibe ambos. Luego valida que el token haya sido firmado con el mismo secreto que conoce el Document Server y que el contenido firmado coincida con la configuracion recibida.

Si algo no cuadra, suelen aparecer errores como:

- token invalido
- token con formato incorrecto
- fallo al abrir el documento

## Papel del frontend

El frontend no firma el token ni lo modifica. Solo hace esto:

1. Pide `GET /api/v1/onlyoffice/config/{documentId}` al backend.
2. Recibe `config` con `token`.
3. Pasa ese objeto al componente `document-editor` de ONLYOFFICE.

Eso significa que toda la responsabilidad criptografica esta en el backend.

## Resumen rapido

- El token se genera en `GenerateToken(config)` dentro de `OnlyOfficeController`.
- Usa `JwtSecret` como secreto compartido.
- Firma con `HS256`.
- El payload contiene `document`, `documentType` y `editorConfig`.
- El resultado se guarda en `config.Token`.
- El backend devuelve `config` completo al frontend.
- El frontend entrega ese `config` a ONLYOFFICE sin alterarlo.
- ONLYOFFICE valida el token antes de aceptar la configuracion.