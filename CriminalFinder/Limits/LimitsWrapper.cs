using YamlDotNet.Serialization;

namespace CriminalChecker.Limits;

public class LimitsWrapper
{
    [YamlMember(Alias = "limits")]
    public Limit[] Limits { get; set; }
}