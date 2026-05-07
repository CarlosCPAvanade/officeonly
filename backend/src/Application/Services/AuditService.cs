using System.Text.Json;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class AuditService : IAuditService
{
    private readonly IAppDbContext _dbContext;

    public AuditService(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task WriteAsync(Guid? userId, Guid? documentId, AuditActionType actionType, string description, object? metadata, string ipAddress, CancellationToken cancellationToken = default)
    {
        _dbContext.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            DocumentId = documentId,
            ActionType = actionType,
            Description = description,
            MetadataJson = metadata == null ? string.Empty : JsonSerializer.Serialize(metadata),
            IpAddress = ipAddress,
            CreatedAtUtc = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
