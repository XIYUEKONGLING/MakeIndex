namespace MakeIndex.Models;

[Serializable]
public class IndexMetaData
{
    public string? BasicDirectory { get; set; } = null;
    public string? CreatedAt { get; set; } = null;
    public long Timestamp { get; set; } = 0;
    public bool HashesIncluded { get; set; } = false;
}