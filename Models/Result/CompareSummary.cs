using Newtonsoft.Json;

namespace MakeIndex.Models.Result;

[Serializable, JsonObject]
public class CompareSummary
{
    [JsonProperty]
    public int TotalChanges { get; set; }
    
    [JsonProperty]
    public int FilesAdded { get; set; }
    
    [JsonProperty]
    public int FilesModified { get; set; }
    
    [JsonProperty]
    public int FilesDeleted { get; set; }
    
    [JsonProperty]
    public int DirectoriesAdded { get; set; }
    
    [JsonProperty]
    public int DirectoriesDeleted { get; set; }
}