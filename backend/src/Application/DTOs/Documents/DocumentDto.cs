namespace Application.DTOs.Documents;

public class DocumentDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public int CurrentVersionNumber { get; set; }
    public long SizeInBytes { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public bool CanRead { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}
