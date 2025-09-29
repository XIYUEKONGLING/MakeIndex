namespace MakeIndex.Models;

[Serializable]
public class IndexFile
{
    public string? Path { get; set; }
    
    public long Size { get; set; }
    public long ModifiedTime { get; set; }
    public long CreatedTime { get; set; }
    public string? Type { get; set; } = null;
    public Dictionary<string, string>? Hashes { get; set; } = null;
}