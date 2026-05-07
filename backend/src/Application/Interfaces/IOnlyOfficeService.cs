using Application.DTOs.OnlyOffice;

namespace Application.Interfaces;

public interface IOnlyOfficeService
{
    Task<OnlyOfficeEditorConfigDto> BuildEditorConfigAsync(Guid documentId, Guid userId, string roleName, CancellationToken cancellationToken = default);
    Task<object> ProcessCallbackAsync(Guid documentId, OnlyOfficeCallbackDto request, string? authorizationHeader, CancellationToken cancellationToken = default);
}
