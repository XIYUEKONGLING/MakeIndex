using Newtonsoft.Json;

namespace MakeIndex.Models;

[Serializable, JsonObject]
public class Index
{
    [JsonProperty]
    public Version Version { get; set; } = new();
    
    [JsonProperty]
    public IndexMetaData MetaData { get; set; } = new();
    
    [JsonProperty]
    public Dictionary<string, IndexFile> Files { get; set; } = new();
    
    [JsonProperty]
    public Dictionary<string, IndexDirectory> Directories { get; set; } = new();
}
