using Newtonsoft.Json;

namespace MakeIndex.Models.Result;

[Serializable, JsonObject]
public class IndexResult
{
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public string? Id { get; set; }
    
    [JsonProperty]
    public long FileCount { get; set; }
    
    [JsonProperty]
    public long DirectoryCount { get; set; }
    
    [JsonProperty]
    public long TotalSize { get; set; }
    
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public string? IndexPath { get; set; }
    
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public string? CreatedAt { get; set; }
}