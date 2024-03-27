using System.Text.Json.Serialization;

namespace ASFPasswordChanger.Data;
public sealed record HashCodeResponse
{
    [JsonPropertyName("hash")]
    public string? Hash { get; set; }

    [JsonPropertyName("errorMsg")]
    public string? ErrorMsg { get; set; }
}
