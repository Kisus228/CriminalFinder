namespace CriminalChecker.Limits;

public enum LimitResultType
{
    AllIsOk,
    BaseLimitViolated,
    ChunkLimitViolated,
    BaseAndChunkLimitsViolated
}