using Domain.Enums;

namespace Domain.Entities;

public class Document
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string CurrentFilePath { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long SizeInBytes { get; set; }
    public DocumentFileType FileType { get; set; }
    public int CurrentVersionNumber { get; set; } = 1;
    public Guid CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public bool IsDeleted { get; set; }
    public ICollection<DocumentVersion> Versions { get; set; } = new List<DocumentVersion>();
    public ICollection<DocumentPermission> Permissions { get; set; } = new List<DocumentPermission>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
