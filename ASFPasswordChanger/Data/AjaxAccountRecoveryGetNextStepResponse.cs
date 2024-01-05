using Newtonsoft.Json;

namespace ASFPasswordChanger.Data;
public sealed record AjaxAccountRecoveryGetNextStepResponse
{
    [JsonProperty(PropertyName = "redirect")]
    public string? Redirect { get; set; }
}
