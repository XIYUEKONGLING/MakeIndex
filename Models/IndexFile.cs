using Newtonsoft.Json;

namespace MakeIndex.Models;

[Serializable, JsonObject]
public class IndexFile
{
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public string? Path { get; set; }
    
    [JsonProperty]
    public long Size { get; set; }
    
    [JsonProperty]
    public long ModifiedTime { get; set; }
    
    [JsonProperty]
    public long CreatedTime { get; set; }
    
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public string? Type { get; set; } = null;
    
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public Dictionary<string, string>? Hashes { get; set; } = null;
}