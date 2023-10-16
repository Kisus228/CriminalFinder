using YamlDotNet.Serialization;

namespace CriminalChecker.Limits;

public class Limit
{
    [YamlMember(Alias = "name")]
    public string Name { get; set; }
    
    [YamlMember(Alias = "ids")]
    public string[] IDs { get; set; }
    
    [YamlMember(Alias = "totalCount")]
    public int TotalCount { get; set; }
    
    [YamlMember(Alias = "chunkCount")]
    public int? ChunkCount { get; set; }
}