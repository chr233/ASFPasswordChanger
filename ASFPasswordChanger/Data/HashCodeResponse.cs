using Newtonsoft.Json;

namespace ASFPasswordChanger.Data;
public sealed record HashCodeResponse
{
    [JsonProperty(PropertyName = "hash")]
    public string? Hash { get; set; }

    [JsonProperty(PropertyName = "errorMsg")]
    public string? ErrorMsg { get; set; }
}
