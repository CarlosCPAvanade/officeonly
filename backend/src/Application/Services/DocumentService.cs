using Application.DTOs.Documents;
using Application.Exceptions;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class DocumentService : IDocumentService
{
    private static readonly Dictionary<string, (DocumentFileType FileType, string MimeType)> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        [".docx"] = (DocumentFileType.Docx, "application/vnd.openxmlformats-officedocument.wordprocessingml.document"),
        [".xlsx"] = (DocumentFileType.Xlsx, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"),
        [".pptx"] = (DocumentFileType.Pptx, "application/vnd.openxmlformats-officedocument.presentationml.presentation")
    };

    private readonly IDocumentRepository _documentRepository;
    private readonly IAppDbContext _dbContext;
    private readonly IFileStorageService _fileStorageService;
    private readonly IAuditService _auditService;
    private readonly IJwtTokenService _jwtTokenService;

    public DocumentService(
        IDocumentRepository documentRepository,
        IAppDbContext dbContext,
        IFileStorageService fileStorageService,
        IAuditService auditService,
        IJwtTokenService jwtTokenService)
    {
        _documentRepository = documentRepository;
        _dbContext = dbContext;
        _fileStorageService = fileStorageService;
        _auditService = auditService;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<IReadOnlyCollection<DocumentDto>> GetDocumentsAsync(Guid userId, string roleName, CancellationToken cancellationToken = default)
    {
        var documents = await _documentRepository.GetAccessibleDocumentsAsync(userId, roleName, cancellationToken);
        return documents.Select(document => MapDocument(document, userId, roleName)).ToArray();
    }

    public async Task<DocumentDetailDto> GetDocumentAsync(Guid documentId, Guid userId, string roleName, CancellationToken cancellationToken = default)
    {
        var document = await GetAccessibleDocumentAsync(documentId, userId, roleName, cancellationToken);
        var result = new DocumentDetailDto();
        CopyDocument(MapDocument(document, userId, roleName), result);
        result.MimeType = document.MimeType;
        result.Versions = document.Versions
            .OrderByDescending(v => v.VersionNumber)
            .Select(v => new DocumentVersionDto
            {
                Id = v.Id,
                VersionNumber = v.VersionNumber,
                SizeInBytes = v.SizeInBytes,
                CreatedAtUtc = v.CreatedAtUtc,
                CreatedBy = v.CreatedByUser?.UserName ?? string.Empty,
                ChangeSummary = v.ChangeSummary
            })
            .ToArray();

        return result;
    }

    public async Task<UploadDocumentResultDto> UploadAsync(IFormFile file, Guid userId, string roleName, CancellationToken cancellationToken = default)
    {
        EnsureUploadAllowed(roleName);

        if (file.Length <= 0)
        {
            throw new AppException("El archivo está vacío.");
        }

        var extension = Path.GetExtension(file.FileName);
        if (!SupportedExtensions.TryGetValue(extension, out var fileMetadata))
        {
            throw new AppException("Formato no soportado. Solo se permiten DOCX, XLSX y PPTX.");
        }

        var documentId = Guid.NewGuid();
        var sanitizedName = Path.GetFileName(file.FileName);
        var currentRelativePath = $"documents/current/{documentId}{extension}";
        var versionRelativePath = $"documents/versions/{documentId}/v1{extension}";

        await using (var stream = file.OpenReadStream())
        {
            await _fileStorageService.SaveAsync(stream, "documents/current", $"{documentId}{extension}", cancellationToken);
        }

        await _fileStorageService.CopyAsync(currentRelativePath, versionRelativePath, cancellationToken);

        var document = new Document
        {
            Id = documentId,
            Title = Path.GetFileNameWithoutExtension(sanitizedName),
            OriginalFileName = sanitizedName,
            CurrentFilePath = currentRelativePath,
            MimeType = fileMetadata.MimeType,
            SizeInBytes = file.Length,
            FileType = fileMetadata.FileType,
            CurrentVersionNumber = 1,
            CreatedByUserId = userId,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
            Permissions = new List<DocumentPermission>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CanRead = true,
                    CanEdit = true,
                    CanDelete = true,
                    CreatedAtUtc = DateTime.UtcNow
                }
            },
            Versions = new List<DocumentVersion>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    VersionNumber = 1,
                    FilePath = versionRelativePath,
                    SizeInBytes = file.Length,
                    CreatedByUserId = userId,
                    CreatedAtUtc = DateTime.UtcNow,
                    ChangeSummary = "Versión inicial"
                }
            }
        };

        await _documentRepository.AddAsync(document, cancellationToken);
        await _documentRepository.SaveChangesAsync(cancellationToken);

        await _auditService.WriteAsync(userId, documentId, AuditActionType.Upload, $"Carga del documento {document.Title}", new { document.Title, document.OriginalFileName }, string.Empty, cancellationToken);
        await _auditService.WriteAsync(userId, documentId, AuditActionType.VersionCreated, $"Versión v1 creada para {document.Title}", new { Version = 1 }, string.Empty, cancellationToken);

        return new UploadDocumentResultDto
        {
            DocumentId = document.Id,
            VersionNumber = 1,
            Title = document.Title
        };
    }

    public async Task DeleteAsync(Guid documentId, Guid userId, string roleName, CancellationToken cancellationToken = default)
    {
        var document = await GetAccessibleDocumentAsync(documentId, userId, roleName, cancellationToken);
        var permission = GetPermission(document, userId, roleName);
        if (!permission.CanDelete)
        {
            throw new ForbiddenException("No tiene permisos para eliminar este documento.");
        }

        document.IsDeleted = true;
        document.UpdatedAtUtc = DateTime.UtcNow;
        await _documentRepository.SaveChangesAsync(cancellationToken);

        await _auditService.WriteAsync(userId, documentId, AuditActionType.Delete, $"Documento eliminado: {document.Title}", null, string.Empty, cancellationToken);
    }

    public async Task<(Stream Stream, string FileName, string ContentType)> DownloadAsync(Guid documentId, Guid? userId, string? roleName, string? accessToken, CancellationToken cancellationToken = default)
    {
        var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken) ?? throw new NotFoundException("Documento no encontrado.");
        if (document.IsDeleted)
        {
            throw new NotFoundException("Documento no encontrado.");
        }

        var isAuthorizedByToken = false;
        Guid? tokenUserId = null;
        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            isAuthorizedByToken = _jwtTokenService.TryValidateDownloadToken(accessToken, documentId, out tokenUserId);
        }

        if (!isAuthorizedByToken)
        {
            if (!userId.HasValue || string.IsNullOrWhiteSpace(roleName))
            {
                throw new UnauthorizedException("Se requiere autenticación.");
            }

            var accessible = await GetAccessibleDocumentAsync(documentId, userId.Value, roleName, cancellationToken);
            var permission = GetPermission(accessible, userId.Value, roleName);
            if (!permission.CanRead)
            {
                throw new ForbiddenException("No tiene permisos para descargar este documento.");
            }
        }

        var stream = await _fileStorageService.OpenReadAsync(document.CurrentFilePath, cancellationToken);

        await _auditService.WriteAsync(tokenUserId ?? userId, document.Id, AuditActionType.Download, $"Descarga del documento {document.Title}", null, string.Empty, cancellationToken);

        return (stream, document.OriginalFileName, document.MimeType);
    }

    public async Task<IReadOnlyCollection<DocumentVersionDto>> GetVersionsAsync(Guid documentId, Guid userId, string roleName, CancellationToken cancellationToken = default)
    {
        var document = await GetAccessibleDocumentAsync(documentId, userId, roleName, cancellationToken);
        return document.Versions
            .OrderByDescending(x => x.VersionNumber)
            .Select(v => new DocumentVersionDto
            {
                Id = v.Id,
                VersionNumber = v.VersionNumber,
                SizeInBytes = v.SizeInBytes,
                CreatedAtUtc = v.CreatedAtUtc,
                CreatedBy = v.CreatedByUser?.UserName ?? string.Empty,
                ChangeSummary = v.ChangeSummary
            })
            .ToArray();
    }

    private async Task<Document> GetAccessibleDocumentAsync(Guid documentId, Guid userId, string roleName, CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetAccessibleByIdAsync(documentId, userId, roleName, cancellationToken);
        return document ?? throw new NotFoundException("Documento no encontrado o sin permisos.");
    }

    private static void EnsureUploadAllowed(string roleName)
    {
        if (!roleName.Equals("admin", StringComparison.OrdinalIgnoreCase) && !roleName.Equals("editor", StringComparison.OrdinalIgnoreCase))
        {
            throw new ForbiddenException("No tiene permisos para subir documentos.");
        }
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

    private static DocumentDto MapDocument(Document document, Guid userId, string roleName)
    {
        var permission = GetPermission(document, userId, roleName);
        return new DocumentDto
        {
            Id = document.Id,
            Title = document.Title,
            OriginalFileName = document.OriginalFileName,
            FileType = document.FileType.ToString().ToLowerInvariant(),
            CurrentVersionNumber = document.CurrentVersionNumber,
            SizeInBytes = document.SizeInBytes,
            CreatedAtUtc = document.CreatedAtUtc,
            UpdatedAtUtc = document.UpdatedAtUtc,
            CreatedBy = document.CreatedByUser?.UserName ?? string.Empty,
            CanRead = permission.CanRead,
            CanEdit = permission.CanEdit,
            CanDelete = permission.CanDelete
        };
    }

    private static void CopyDocument(DocumentDto source, DocumentDetailDto target)
    {
        target.Id = source.Id;
        target.Title = source.Title;
        target.OriginalFileName = source.OriginalFileName;
        target.FileType = source.FileType;
        target.CurrentVersionNumber = source.CurrentVersionNumber;
        target.SizeInBytes = source.SizeInBytes;
        target.CreatedAtUtc = source.CreatedAtUtc;
        target.UpdatedAtUtc = source.UpdatedAtUtc;
        target.CreatedBy = source.CreatedBy;
        target.CanRead = source.CanRead;
        target.CanEdit = source.CanEdit;
        target.CanDelete = source.CanDelete;
    }
}
