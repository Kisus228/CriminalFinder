using UrodChecker.BlockAmountCalculations;
using UrodChecker.Nbt;

namespace UrodChecker;

public class Schematica
{
    public int MinX { get; }
    public int MinY { get; }
    public int MinZ { get; }
    
    private readonly BlockData[,,] blockDatas;
    private readonly Dictionary<short, string> blockMapping;
    private readonly short width;
    private readonly short length;
    private readonly short height;
    
    public Schematica(NbtParseResult nbtParseResult)
    {
        blockMapping = nbtParseResult.BlockMapping;

        byte[]? extraBlocks = null;
        var extraBlocksNibble = nbtParseResult.ExtraBlocksNibble;
        if (extraBlocksNibble is not null) {
            extraBlocks = new byte[extraBlocksNibble.Length * 2];
            for (var i = 0; i < extraBlocksNibble.Length; i++) {
                extraBlocks[i * 2 + 0] = (byte) ((extraBlocksNibble[i] >> 4) & 0xF);
                extraBlocks[i * 2 + 1] = (byte) (extraBlocksNibble[i] & 0xF);
            }
        } else if (nbtParseResult.ExtraBlocks is not null) {
            extraBlocks = nbtParseResult.ExtraBlocks;
        }

        width = nbtParseResult.Width;
        length = nbtParseResult.Length;
        height = nbtParseResult.Height;

        MinX = nbtParseResult.MinX;
        MinY = nbtParseResult.MinY;
        MinZ = nbtParseResult.MinZ;

        blockDatas = new BlockData[width, height, length];

        
        for (var x = 0; x < width; x++) {
            for (var y = 0; y < height; y++) {
                for (var z = 0; z < length; z++) {
                    var index = x + (y * length + z) * width;
                    var blockID = (nbtParseResult.LocalBlocks[index] & 0xFF) | (extraBlocks is not null ? (extraBlocks[index] & 0xFF) << 8 : 0);
                    var meta = nbtParseResult.LocalMetadata[index] & 0xFF;
                    
                    blockDatas[x, y, z] = new BlockData(blockID, meta);
                }
            }
        }
    }
    
    public BlockAmountMappingManager GetBlockAmountMapping()
    {
        var mappingManager = new BlockAmountMappingManager(width / 16, length / 16);
        for (var x = 0; x < width; x++) {
            for (var y = 0; y < height; y++) {
                for (var z = 0; z < length; z++)
                {
                    var chunkX = x / 16;
                    var chunkZ = z / 16;
                    
                    var blockData = blockDatas[x, y, z];
                    var idMeta = $"{blockData.ID}:{blockData.Meta}";
                    
                    mappingManager.IncrementAmount(idMeta, new ValueTuple<int, int>(chunkX, chunkZ));
                }
            }
        }

        return mappingManager;
    }
}