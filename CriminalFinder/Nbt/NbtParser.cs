using fNbt;

namespace UrodChecker.Nbt;

public static class NbtParser
{
    private const string BlocksTagName = "Blocks";
    private const string DataTagName = "Data";
    private const string AddBlocksTagName = "AddBlocks";
    private const string AddTagName = "Add";
    private const string WidthTagName = "Width";
    private const string HeightTagName = "Height";
    private const string LengthTagName = "Length";
    private const string SchematicaMappingTagName = "SchematicaMapping";
    private const string XTagName = "x";
    private const string YTagName = "y";
    private const string ZTagName = "z";
    
    public static NbtParseResult FromFile(string filePath)
    {
        var nbtFile = new NbtFile();
        nbtFile.LoadFromFile(filePath);
        var rootTag = nbtFile.RootTag;
        
        var mappingTag = rootTag.Get<NbtCompound>(SchematicaMappingTagName)!;

        return new NbtParseResult
        {
            LocalBlocks = rootTag.Get(BlocksTagName)!.ByteArrayValue,
            LocalMetadata = rootTag.Get(DataTagName)!.ByteArrayValue,
            ExtraBlocksNibble = rootTag.Get(AddBlocksTagName)?.ByteArrayValue,
            ExtraBlocks = rootTag.Get(AddTagName)?.ByteArrayValue,
            Width = rootTag.Get(WidthTagName)!.ShortValue,
            Height = rootTag.Get(HeightTagName)!.ShortValue,
            Length = rootTag.Get(LengthTagName)!.ShortValue,
            BlockMapping = mappingTag.Tags.ToDictionary(tag => tag.ShortValue, tag => tag.Name),
            MinX = rootTag.Get(XTagName)!.IntValue,
            MinY = rootTag.Get(YTagName)!.IntValue,
            MinZ = rootTag.Get(ZTagName)!.IntValue
        };
    }
}