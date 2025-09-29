using Newtonsoft.Json;

namespace MakeIndex.Models;

[Serializable, JsonObject]
public class IndexDirectory
{
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public string? Path { get; set; }
    
    [JsonProperty]
    public long ModifiedTime { get; set; }
    
    [JsonProperty]
    public long CreatedTime { get; set; }
}