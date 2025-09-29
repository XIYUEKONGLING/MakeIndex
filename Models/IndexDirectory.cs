namespace MakeIndex.Models;

[Serializable]
public class IndexDirectory
{
    public string? Path { get; set; }
    
    public long ModifiedTime { get; set; }
    public long CreatedTime { get; set; }
}