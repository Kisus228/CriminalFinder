namespace UrodChecker.BlockAmountCalculations;

public class BlockAmountMapping
{
    public int[,] ChunksAmount { get; init; } = new int[0, 0];
    public int TotalAmount { get; private set; } = 0;

    public void IncrementChunkAmount(int firstIndex, int secondIndex)
    {
        ChunksAmount[firstIndex, secondIndex]++;
        TotalAmount++;
    }

    public BlockAmountMapping Concat(BlockAmountMapping mappingToConcat)
    {
        var xLength = ChunksAmount.GetLength(0);
        var zLength = ChunksAmount.GetLength(1);
            
        var newChunkAmount = new int[xLength, zLength];
        for (var x = 0; x < xLength; x++)
        {
            for (var z = 0; z < zLength; z++)
            {
                newChunkAmount[x, z] = ChunksAmount[x, z] + mappingToConcat.ChunksAmount[x, z];
            }
        }

        return new BlockAmountMapping
        {
            ChunksAmount = newChunkAmount,
            TotalAmount = TotalAmount + mappingToConcat.TotalAmount
        };
    }
}