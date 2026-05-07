namespace Application.DTOs.Documents;

public class DocumentDetailDto : DocumentDto
{
    public string MimeType { get; set; } = string.Empty;
    public IEnumerable<DocumentVersionDto> Versions { get; set; } = Array.Empty<DocumentVersionDto>();
}
