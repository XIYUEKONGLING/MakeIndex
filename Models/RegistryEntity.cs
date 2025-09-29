using Newtonsoft.Json;

namespace MakeIndex.Models;

[Serializable, JsonObject]
public class RegistryEntity
{
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public string? Id { get; set; } = null;
    
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public string? FileName { get; set; } = null;
    
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public string? BasicDirectory { get; set; } = null;
    
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public string? CreatedAt { get; set; } = null;
    
    [JsonProperty]
    public long Timestamp { get; set; } = 0;
    
    [JsonProperty]
    public bool Binaries { get; set; } = false;
    
    [JsonProperty]
    public long FileCount { get; set; } = 0;
    
    [JsonProperty]
    public long FileSize { get; set; } = 0;
    
    [JsonProperty]
    public bool HashesIncluded { get; set; } = false;
}