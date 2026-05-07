using System.Text.Json.Serialization;

namespace Application.DTOs.OnlyOffice;

public class OnlyOfficeCallbackDto
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("users")]
    public List<string> Users { get; set; } = new();

    [JsonPropertyName("actions")]
    public List<OnlyOfficeCallbackActionDto> Actions { get; set; } = new();

    [JsonPropertyName("history")]
    public object? History { get; set; }

    [JsonPropertyName("token")]
    public string? Token { get; set; }
}

public class OnlyOfficeCallbackActionDto
{
    [JsonPropertyName("type")]
    public int Type { get; set; }

    [JsonPropertyName("userid")]
    public string UserId { get; set; } = string.Empty;
}
