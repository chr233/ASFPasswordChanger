using Newtonsoft.Json;

namespace ASFPasswordChanger.Data;
public sealed record AjaxPollAccountRecoveryConfirmationResponse
{
    [JsonProperty(PropertyName = "continue")]
    public bool Continue { get; set; }

    [JsonProperty(PropertyName = "success")]
    public bool Success { get; set; }
}
