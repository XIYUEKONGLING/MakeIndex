using Newtonsoft.Json;

namespace MakeIndex.Models;

[Serializable, JsonObject]
public class Registry
{
    [JsonProperty]
    public Version Version { get; set; } = new();
    
    [JsonProperty]
    public List<RegistryEntity> Entities { get; set; } = new(); 
}