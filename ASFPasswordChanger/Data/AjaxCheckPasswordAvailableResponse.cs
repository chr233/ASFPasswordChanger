using Newtonsoft.Json;

namespace ASFPasswordChanger.Data;
public sealed record AjaxCheckPasswordAvailableResponse
{
    [JsonProperty(PropertyName = "available")]
    public bool Available { get; set; }
}
