using System.Text.Json.Serialization;

namespace ASFPasswordChanger.Data;
public sealed record AjaxCheckPasswordAvailableResponse
{
    [JsonPropertyName("available")]
    public bool Available { get; set; }
}
