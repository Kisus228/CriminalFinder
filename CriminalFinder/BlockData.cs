namespace CriminalChecker;

public class BlockData
{
    public int ID { get; }
    public int Meta { get; }

    public BlockData(int id, int meta)
    {
        ID = id;
        Meta = meta;
    }
}