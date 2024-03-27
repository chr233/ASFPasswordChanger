using System.Text.Json.Serialization;

namespace ASFPasswordChanger.Data;
public sealed record AjaxAccountRecoveryGetNextStepResponse
{
    [JsonPropertyName("redirect")]
    public string? Redirect { get; set; }
}
