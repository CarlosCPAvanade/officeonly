namespace Application.DTOs.Documents;

public class DocumentVersionDto
{
    public Guid Id { get; set; }
    public int VersionNumber { get; set; }
    public long SizeInBytes { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string ChangeSummary { get; set; } = string.Empty;
}
