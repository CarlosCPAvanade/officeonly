namespace Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public Guid RoleId { get; set; }
    public Role? Role { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; }
    public ICollection<Document> CreatedDocuments { get; set; } = new List<Document>();
    public ICollection<DocumentVersion> CreatedVersions { get; set; } = new List<DocumentVersion>();
    public ICollection<DocumentPermission> DocumentPermissions { get; set; } = new List<DocumentPermission>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
