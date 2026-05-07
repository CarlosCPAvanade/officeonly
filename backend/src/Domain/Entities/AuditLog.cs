using Domain.Enums;

namespace Domain.Entities;

public class AuditLog
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public User? User { get; set; }
    public Guid? DocumentId { get; set; }
    public Document? Document { get; set; }
    public AuditActionType ActionType { get; set; }
    public string Description { get; set; } = string.Empty;
    public string MetadataJson { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
