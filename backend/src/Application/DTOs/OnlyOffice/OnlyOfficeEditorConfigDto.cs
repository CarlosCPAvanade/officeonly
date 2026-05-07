namespace Application.DTOs.OnlyOffice;

public class OnlyOfficeEditorConfigDto
{
    public string DocumentType { get; set; } = string.Empty;
    public string Type { get; set; } = "desktop";
    public object Document { get; set; } = new();
    public object EditorConfig { get; set; } = new();
    public string Token { get; set; } = string.Empty;
}
