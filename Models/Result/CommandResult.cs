using Newtonsoft.Json;

namespace MakeIndex.Models.Result;

[Serializable, JsonObject]
public class CommandResult
{
    [JsonProperty]
    public bool Success { get; set; }
    
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public string? Message { get; set; }
    
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public string? Error { get; set; }
    
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public object? Data { get; set; }
    
    [JsonProperty]
    public long ExecutionTime { get; set; }
    
    public static CommandResult Ok(string? message = null, object? data = null) => new()
    {
        Success = true,
        Message = message,
        Data = data,
        ExecutionTime = 0
    };
    
    public static CommandResult Fail(string error, object? data = null) => new()
    {
        Success = false,
        Error = error,
        Data = data,
        ExecutionTime = 0
    };
}