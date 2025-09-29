namespace MakeIndex.Models;

[Serializable]
public class Index
{
    public Version Version { get; set; } = new();
    public IndexMetaData MetaData { get; set; } = new();
    public Dictionary<string, IndexFile> Files { get; set; } = new();
    public Dictionary<string, IndexDirectory> Directories { get; set; } = new();
}
