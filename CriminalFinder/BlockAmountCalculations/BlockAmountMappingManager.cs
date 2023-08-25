namespace CriminalChecker.BlockAmountCalculations;

public class BlockAmountMappingManager
{
    private readonly Dictionary<string, BlockAmountMapping> blockAmountMapping = new();
    private int chunkXLength;
    private int chunkZLength;

    public BlockAmountMappingManager(int chunkXLength, int chunkZLength)
    {
        this.chunkXLength = chunkXLength;
        this.chunkZLength = chunkZLength;
    }
    
    public void IncrementAmount(string idMeta, (int FirstIndex, int SecondIndex) chunkCoords)
    {
        if (blockAmountMapping.TryGetValue(idMeta, out var mapping))
        {
            mapping.IncrementChunkAmount(chunkCoords.FirstIndex, chunkCoords.SecondIndex);
        }
        else
        {
            var newMapping = new BlockAmountMapping
            {
                ChunksAmount = new int[chunkXLength,chunkZLength]
            };
            newMapping.IncrementChunkAmount(chunkCoords.FirstIndex, chunkCoords.SecondIndex);
            blockAmountMapping.Add(idMeta, newMapping);
        }
    }

    public BlockAmountMapping GetBlockAmountMapping(string idMeta)
    {
        return blockAmountMapping[idMeta];
    }

    public bool ContainsBlock(string idMeta)
    {
        return blockAmountMapping.ContainsKey(idMeta);
    }
}

