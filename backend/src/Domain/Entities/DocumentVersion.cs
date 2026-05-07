namespace Domain.Entities;

public class DocumentVersion
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public Document? Document { get; set; }
    public int VersionNumber { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public long SizeInBytes { get; set; }
    public Guid CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string ChangeSummary { get; set; } = string.Empty;
}
