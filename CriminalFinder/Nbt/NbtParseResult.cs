namespace UrodChecker.Nbt;

public class NbtParseResult
{
    public byte[] LocalBlocks { get; init; }
    public byte[] LocalMetadata { get; init; }
    public byte[]? ExtraBlocksNibble { get; init; }
    public byte[]? ExtraBlocks { get; init; }
    public short Width { get; init; }
    public short Height { get; init; }
    public short Length { get; init; }
    public Dictionary<short, string> BlockMapping { get; init; }
    public int MinX { get; init; }
    public int MinY { get; init; }
    public int MinZ { get; init; }
}