namespace Application.Options;

public class OnlyOfficeOptions
{
    public const string SectionName = "OnlyOffice";
    public string DocumentServerUrl { get; set; } = string.Empty;
    public string InternalDocumentServerUrl { get; set; } = string.Empty;
    public string JwtSecret { get; set; } = string.Empty;
    public string InternalApiBaseUrl { get; set; } = string.Empty;
    public string PublicApiBaseUrl { get; set; } = string.Empty;
    public int UrlExpirationMinutes { get; set; } = 20;
}
