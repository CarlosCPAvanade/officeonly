using System.Net.Http.Headers;
using System.Net.Http;
using Application.DTOs.OnlyOffice;
using Application.Exceptions;
using Application.Interfaces;
using Application.Options;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Application.Services;

public class OnlyOfficeService : IOnlyOfficeService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAppDbContext _dbContext;
    private readonly IFileStorageService _fileStorageService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IAuditService _auditService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly OnlyOfficeOptions _onlyOfficeOptions;

    public OnlyOfficeService(
        IDocumentRepository documentRepository,
        IUserRepository userRepository,
        IAppDbContext dbContext,
        IFileStorageService fileStorageService,
        IJwtTokenService jwtTokenService,
        IAuditService auditService,
        IHttpClientFactory httpClientFactory,
        IOptions<OnlyOfficeOptions> onlyOfficeOptions)
    {
        _documentRepository = documentRepository;
        _userRepository = userRepository;
        _dbContext = dbContext;
        _fileStorageService = fileStorageService;
        _jwtTokenService = jwtTokenService;
        _auditService = auditService;
        _httpClientFactory = httpClientFactory;
        _onlyOfficeOptions = onlyOfficeOptions.Value;
    }

    public async Task<OnlyOfficeEditorConfigDto> BuildEditorConfigAsync(Guid documentId, Guid userId, string roleName, CancellationToken cancellationToken = default)
    {
        var document = await _documentRepository.GetAccessibleByIdAsync(documentId, userId, roleName, cancellationToken)
            ?? throw new NotFoundException("Documento no encontrado o sin permisos.");
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken) ?? throw new UnauthorizedException("Usuario inválido.");

        var permission = GetPermission(document, userId, roleName);
        if (!permission.CanRead)
        {
            throw new ForbiddenException("No tiene permisos para abrir este documento.");
        }

        var editorMode = permission.CanEdit ? "edit" : "view";
        var fileExtension = Path.GetExtension(document.OriginalFileName).TrimStart('.').ToLowerInvariant();
        var expiresAt = DateTime.UtcNow.AddMinutes(_onlyOfficeOptions.UrlExpirationMinutes);
        var downloadToken = _jwtTokenService.GenerateDownloadToken(document.Id, user.Id, expiresAt);
        var documentUrl = $"{_onlyOfficeOptions.InternalApiBaseUrl.TrimEnd('/')}/api/documents/{document.Id}/download?accessToken={Uri.EscapeDataString(downloadToken)}";
        var callbackUrl = $"{_onlyOfficeOptions.InternalApiBaseUrl.TrimEnd('/')}/api/onlyoffice/callback/{document.Id}";

        var payload = new
        {
            document = new
            {
                fileType = fileExtension,
                key = $"{document.Id:N}-v{document.CurrentVersionNumber}",
                title = document.OriginalFileName,
                url = documentUrl,
                permissions = new
                {
                    edit = permission.CanEdit,
                    download = true,
                    comment = permission.CanEdit,
                    review = permission.CanEdit,
                    print = true
                }
            },
            documentType = ResolveDocumentType(document.OriginalFileName),
            editorConfig = new
            {
                callbackUrl,
                mode = editorMode,
                lang = "es",
                user = new
                {
                    id = user.Id.ToString(),
                    name = user.UserName
                },
                customization = new
                {
                    autosave = true,
                    forcesave = true
                }
            },
            type = "desktop"
        };

        return new OnlyOfficeEditorConfigDto
        {
            DocumentType = ResolveDocumentType(document.OriginalFileName),
            Type = "desktop",
            Document = payload.document,
            EditorConfig = payload.editorConfig,
            Token = _jwtTokenService.GenerateOnlyOfficeToken(payload)
        };
    }

    public async Task<object> ProcessCallbackAsync(Guid documentId, OnlyOfficeCallbackDto request, string? authorizationHeader, CancellationToken cancellationToken = default)
    {
        ValidateCallbackToken(request, authorizationHeader);

        if (request.Status != 2 && request.Status != 6)
        {
            return new { error = 0 };
        }

        if (string.IsNullOrWhiteSpace(request.Url))
        {
            throw new AppException("El callback de ONLYOFFICE no contiene una URL de descarga.");
        }

        var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken) ?? throw new NotFoundException("Documento no encontrado.");
        if (document.IsDeleted)
        {
            throw new NotFoundException("Documento no encontrado.");
        }

        var client = _httpClientFactory.CreateClient(nameof(OnlyOfficeService));
        using var downloadRequest = new HttpRequestMessage(HttpMethod.Get, ResolveCallbackDownloadUrl(request.Url));
        using var response = await client.SendAsync(downloadRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await _fileStorageService.ReplaceAsync(document.CurrentFilePath, responseStream, cancellationToken);

        var extension = Path.GetExtension(document.OriginalFileName);
        var newVersion = document.CurrentVersionNumber + 1;
        var versionPath = $"documents/versions/{document.Id}/v{newVersion}{extension}";
        await _fileStorageService.CopyAsync(document.CurrentFilePath, versionPath, cancellationToken);

        var size = await _fileStorageService.GetSizeAsync(document.CurrentFilePath, cancellationToken);
        var userId = ResolveCallbackUserId(request, document);
        var documentVersion = new DocumentVersion
        {
            Id = Guid.NewGuid(),
            DocumentId = document.Id,
            VersionNumber = newVersion,
            FilePath = versionPath,
            SizeInBytes = size,
            CreatedByUserId = userId,
            CreatedAtUtc = DateTime.UtcNow,
            ChangeSummary = request.Status == 6 ? "Force save desde ONLYOFFICE" : "Edición guardada desde ONLYOFFICE"
        };

        document.CurrentVersionNumber = newVersion;
        document.SizeInBytes = size;
        document.UpdatedAtUtc = DateTime.UtcNow;
        await _dbContext.DocumentVersions.AddAsync(documentVersion, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.WriteAsync(userId, document.Id, AuditActionType.Edit, $"Documento editado en ONLYOFFICE: {document.Title}", new { request.Status, newVersion }, string.Empty, cancellationToken);
        await _auditService.WriteAsync(userId, document.Id, AuditActionType.VersionCreated, $"Versión v{newVersion} creada para {document.Title}", new { Version = newVersion }, string.Empty, cancellationToken);

        return new { error = 0 };
    }

    private string ResolveCallbackDownloadUrl(string requestUrl)
    {
        if (string.IsNullOrWhiteSpace(_onlyOfficeOptions.InternalDocumentServerUrl))
        {
            return requestUrl;
        }

        if (!Uri.TryCreate(requestUrl, UriKind.Absolute, out var sourceUri))
        {
            return requestUrl;
        }

        if (!Uri.TryCreate(_onlyOfficeOptions.InternalDocumentServerUrl, UriKind.Absolute, out var internalBaseUri))
        {
            return requestUrl;
        }

        if (!Uri.TryCreate(_onlyOfficeOptions.DocumentServerUrl, UriKind.Absolute, out var publicBaseUri))
        {
            publicBaseUri = sourceUri;
        }

        var isLocalPublicHost = publicBaseUri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
            || publicBaseUri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase)
            || publicBaseUri.Host.Equals("host.docker.internal", StringComparison.OrdinalIgnoreCase);

        var shouldRewrite = sourceUri.Host.Equals(publicBaseUri.Host, StringComparison.OrdinalIgnoreCase)
            || isLocalPublicHost;

        if (!shouldRewrite)
        {
            return requestUrl;
        }

        var builder = new UriBuilder(sourceUri)
        {
            Scheme = internalBaseUri.Scheme,
            Host = internalBaseUri.Host,
            Port = internalBaseUri.IsDefaultPort ? -1 : internalBaseUri.Port
        };

        return builder.Uri.ToString();
    }

    private void ValidateCallbackToken(OnlyOfficeCallbackDto request, string? authorizationHeader)
    {
        var token = request.Token;
        if (string.IsNullOrWhiteSpace(token) && !string.IsNullOrWhiteSpace(authorizationHeader) && authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            token = authorizationHeader[7..].Trim();
        }

        if (string.IsNullOrWhiteSpace(token) || !_jwtTokenService.ValidateOnlyOfficeToken(token))
        {
            throw new UnauthorizedException("Callback de ONLYOFFICE no autorizado.");
        }
    }

    private static Guid ResolveCallbackUserId(OnlyOfficeCallbackDto request, Document document)
    {
        foreach (var userValue in request.Users)
        {
            if (Guid.TryParse(userValue, out var userId))
            {
                return userId;
            }
        }

        foreach (var action in request.Actions)
        {
            if (Guid.TryParse(action.UserId, out var userId))
            {
                return userId;
            }
        }

        return document.CreatedByUserId;
    }

    private static (bool CanRead, bool CanEdit, bool CanDelete) GetPermission(Document document, Guid userId, string roleName)
    {
        if (roleName.Equals("admin", StringComparison.OrdinalIgnoreCase))
        {
            return (true, true, true);
        }

        var permission = document.Permissions.FirstOrDefault(p => p.UserId == userId);
        if (permission == null)
        {
            return (false, false, false);
        }

        return (permission.CanRead, permission.CanEdit, permission.CanDelete);
    }

    private static string ResolveDocumentType(string fileName)
    {
        return Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".docx" => "word",
            ".xlsx" => "cell",
            ".pptx" => "slide",
            _ => throw new AppException("Tipo de documento no soportado para ONLYOFFICE.")
        };
    }
}
