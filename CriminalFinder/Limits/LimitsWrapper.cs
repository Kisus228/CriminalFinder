using System.Text.Json.Serialization;

namespace UrodChecker;

public class LimitsWrapper
{
    [JsonPropertyName("limits")]
    public Limit[] Limits { get; set; }
}