namespace Application.DTOs.Documents;

public class UploadDocumentResultDto
{
    public Guid DocumentId { get; set; }
    public int VersionNumber { get; set; }
    public string Title { get; set; } = string.Empty;
}
