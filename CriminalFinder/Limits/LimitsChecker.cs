using CriminalChecker.BlockAmountCalculations;

namespace CriminalChecker.Limits;

public static class LimitsChecker
{
    public static IEnumerable<LimitResult> CheckLimits(Limit[] limits,
        BlockAmountMappingManager blockAmountMappingManager)
    {
        foreach (var limit in limits)
        {
            var blockAmountMappings = limit.IDs
                .Where(blockAmountMappingManager.ContainsBlock)
                .Select(blockAmountMappingManager.GetBlockAmountMapping)
                .ToArray();

            if (blockAmountMappings.Length == 0)
            {
                yield return new LimitResult
                {
                    Name = limit.Name,
                    TotalLimit = limit.TotalCount,
                    TotalAmount = 0,
                    ChunkLimit = limit.ChunkCount,
                    BlockAmountMapping = new BlockAmountMapping(),
                    LimitResultType = LimitResultType.AllIsOk
                };
                continue;
            }

            var amountResult = blockAmountMappings
                .Aggregate((x, y) => x.Concat(y));

            yield return new LimitResult
            {
                Name = limit.Name,
                TotalLimit = limit.TotalCount,
                TotalAmount = amountResult.TotalAmount,
                ChunkLimit = limit.ChunkCount,
                BlockAmountMapping = amountResult,
                LimitResultType = GetLimitResultType(limit, amountResult)
            };
        }
    }

    private static LimitResultType GetLimitResultType(Limit limit,
        BlockAmountMapping amountResult)
    {
        var chunkLimitViolated = limit.ChunkCount != null &&
                                 amountResult.ChunksAmount.Cast<int>().Any(amount => amount > limit.ChunkCount);
        var totalLimitViolated = amountResult.TotalAmount > limit.TotalCount;

        if (chunkLimitViolated && totalLimitViolated)
            return LimitResultType.BaseAndChunkLimitsViolated;

        if (chunkLimitViolated)
            return LimitResultType.ChunkLimitViolated;

        if (totalLimitViolated)
            return LimitResultType.BaseLimitViolated;

        return LimitResultType.AllIsOk;
    }
}