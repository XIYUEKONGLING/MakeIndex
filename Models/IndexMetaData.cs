using Newtonsoft.Json;

namespace MakeIndex.Models;

[Serializable, JsonObject]
public class IndexMetaData
{
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public string? BasicDirectory { get; set; } = null;
    
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public string? CreatedAt { get; set; } = null;
    
    [JsonProperty]
    public long Timestamp { get; set; } = 0;
    
    [JsonProperty]
    public bool HashesIncluded { get; set; } = false;
}