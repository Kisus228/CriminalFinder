using System.Text.Json.Serialization;

namespace CriminalChecker.Limits;

public class LimitsWrapper
{
    [JsonPropertyName("limits")]
    public Limit[] Limits { get; set; }
}