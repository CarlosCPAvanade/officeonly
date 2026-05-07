using Application.DTOs.Documents;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces;

public interface IDocumentService
{
    Task<IReadOnlyCollection<DocumentDto>> GetDocumentsAsync(Guid userId, string roleName, CancellationToken cancellationToken = default);
    Task<DocumentDetailDto> GetDocumentAsync(Guid documentId, Guid userId, string roleName, CancellationToken cancellationToken = default);
    Task<UploadDocumentResultDto> UploadAsync(IFormFile file, Guid userId, string roleName, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid documentId, Guid userId, string roleName, CancellationToken cancellationToken = default);
    Task<(Stream Stream, string FileName, string ContentType)> DownloadAsync(Guid documentId, Guid? userId, string? roleName, string? accessToken, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<DocumentVersionDto>> GetVersionsAsync(Guid documentId, Guid userId, string roleName, CancellationToken cancellationToken = default);
}
