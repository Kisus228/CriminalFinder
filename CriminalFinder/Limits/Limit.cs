using System.Text.Json.Serialization;

namespace CriminalChecker.Limits;

public class Limit
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("ids")]
    public string[] IDs { get; set; }
    
    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }
    
    [JsonPropertyName("chunkCount")]
    public int? ChunkCount { get; set; }
}