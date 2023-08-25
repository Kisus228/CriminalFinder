using System.Text.Json.Serialization;

namespace CriminalChecker.Configs;

public class Config
{
    [JsonPropertyName("schematicsPath")]
    public string SchematicsPath { get; set; }
}