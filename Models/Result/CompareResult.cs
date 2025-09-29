using Newtonsoft.Json;

namespace MakeIndex.Models.Result;

[Serializable, JsonObject]
public class CompareResult
{
    [JsonProperty]
    public List<string> NewFiles { get; set; } = new();
    
    [JsonProperty]
    public List<string> ModifiedFiles { get; set; } = new();
    
    [JsonProperty]
    public List<string> DeletedFiles { get; set; } = new();
    
    [JsonProperty]
    public List<string> NewDirectories { get; set; } = new();
    
    [JsonProperty]
    public List<string> DeletedDirectories { get; set; } = new();
    
    [JsonProperty]
    public bool HashVerificationPerformed { get; set; }
    
    [JsonProperty]
    public CompareSummary Summary { get; set; } = new();
}