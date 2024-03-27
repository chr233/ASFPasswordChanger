using System.Text.Json.Serialization;

namespace ASFPasswordChanger.Data;
public sealed record AjaxPollAccountRecoveryConfirmationResponse
{
    [JsonPropertyName("continue")]
    public bool Continue { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }
}
