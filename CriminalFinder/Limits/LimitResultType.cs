using System.ComponentModel;

namespace CriminalChecker.Limits;

public enum LimitResultType
{
    [Description("Всё отлично")] AllIsOk,
    [Description("Нарушен огран по базе")] BaseLimitViolated,

    [Description("Нарушен огран по чанкам")]
    ChunkLimitViolated,

    [Description("Нарушен огран по базе и по чанкам")]
    BaseAndChunkLimitsViolated
}