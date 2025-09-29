using Newtonsoft.Json;

namespace MakeIndex.Models.Result;

public class DirectorySizeResult
{
    [JsonProperty]
    public string Path { get; set; } = string.Empty;
    
    [JsonProperty]
    public long Size { get; set; }
    
    [JsonProperty]
    public string SizeHuman { get; set; } = string.Empty;
    
    [JsonProperty]
    public int Depth { get; set; }
}
