namespace Application.DTOs.Audit;

public class AuditLogDto
{
    public Guid Id { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
