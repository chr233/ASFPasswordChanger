using Newtonsoft.Json;

namespace ASFPasswordChanger.Data;
public sealed record AjaxSendAccountRecoveryCodeResponse
{
    [JsonProperty(PropertyName = "success")]
    public bool Success { get; set; }
}
