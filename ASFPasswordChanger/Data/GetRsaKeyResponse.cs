using System.Text.Json.Serialization;

namespace ASFPasswordChanger.Data;
public sealed record GetRsaKeyResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("publickey_mod")]
    public string? PublicKeyMod { get; set; }

    [JsonPropertyName("publickey_exp")]
    public string? PublicKeyExp { get; set; }

    [JsonPropertyName("timestamp")]
    public string? TimeStamp { get; set; }

    [JsonPropertyName("token_gid")]
    public string? TokenGid { get; set; }
}
