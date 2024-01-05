using Newtonsoft.Json;

namespace ASFPasswordChanger.Data;
public sealed record GetRsaKeyResponse
{
    [JsonProperty(PropertyName = "success")]
    public bool Success { get; set; }

    [JsonProperty(PropertyName = "publickey_mod")]
    public string? PublicKeyMod { get; set; }

    [JsonProperty(PropertyName = "publickey_exp")]
    public string? PublicKeyExp { get; set; }

    [JsonProperty(PropertyName = "timestamp")]
    public string? TimeStamp { get; set; }

    [JsonProperty(PropertyName = "token_gid")]
    public string? TokenGid { get; set; }
}
