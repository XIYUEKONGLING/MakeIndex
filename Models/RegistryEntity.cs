namespace MakeIndex.Models;

[Serializable]
public class RegistryEntity
{
    public string? Id { get; set; } = null;
    public string? FileName { get; set; } = null;
    public string? BasicDirectory { get; set; } = null;
    public string? CreatedAt { get; set; } = null;
    public long Timestamp { get; set; } = 0;
    public long FileCount { get; set; } = 0;
    public long FileSize { get; set; } = 0;
    public bool HashesIncluded { get; set; } = false;
}