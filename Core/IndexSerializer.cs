using System.IO;
using MakeIndex.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Index = MakeIndex.Models.Index;

namespace MakeIndex.Core;

public static class IndexSerializer
{
    public static void Serialize(Index index, string filePath, bool useBson = false, bool indent = true)
    {
        var settings = new JsonSerializerSettings
        {
            Formatting = indent ? Formatting.Indented : Formatting.None,
            NullValueHandling = NullValueHandling.Include
        };

        if (useBson)
            SerializeBson(index, filePath, settings);
        else
            SerializeJson(index, filePath, settings);
    }

    public static Index? Deserialize(string filePath, bool useBson = false)
    {
        var settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Include
        };

        return useBson 
            ? DeserializeBson(filePath, settings) 
            : DeserializeJson(filePath, settings);
    }

    private static void SerializeJson(Index index, string filePath, JsonSerializerSettings settings) => 
        File.WriteAllText(filePath, JsonConvert.SerializeObject(index, settings));

    private static void SerializeBson(Index index, string filePath, JsonSerializerSettings settings)
    {
        using var stream = File.Create(filePath);
        using var writer = new BsonDataWriter(stream);
        JsonSerializer.Create(settings).Serialize(writer, index);
    }

    private static Index? DeserializeJson(string filePath, JsonSerializerSettings settings) => 
        JsonConvert.DeserializeObject<Index>(File.ReadAllText(filePath), settings);

    private static Index? DeserializeBson(string filePath, JsonSerializerSettings settings)
    {
        using var stream = File.OpenRead(filePath);
        using var reader = new BsonDataReader(stream);
        return JsonSerializer.Create(settings).Deserialize<Index>(reader);
    }
}