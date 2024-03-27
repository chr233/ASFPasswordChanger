using System.Text.Json.Serialization;

namespace ASFPasswordChanger.Data;
public sealed record AjaxSendAccountRecoveryCodeResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }
}
