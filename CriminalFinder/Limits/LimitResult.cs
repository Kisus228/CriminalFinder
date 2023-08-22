using UrodChecker.BlockAmountCalculations;

namespace UrodChecker.Limits;

public class LimitResult
{
    public string Name { get; init; }
    public int TotalLimit { get; init; }
    public int TotalAmount { get; init; }
    public int? ChunkLimit { get; init; }
    public BlockAmountMapping BlockAmountMapping { get; init; }
    public LimitResultType LimitResultType { get; init; }
}