using Domain.Enums;

namespace Application.Interfaces;

public interface IAuditService
{
    Task WriteAsync(Guid? userId, Guid? documentId, AuditActionType actionType, string description, object? metadata, string ipAddress, CancellationToken cancellationToken = default);
}
