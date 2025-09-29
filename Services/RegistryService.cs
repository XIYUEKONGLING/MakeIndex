using MakeIndex.Interfaces;
using MakeIndex.Models;
using MakeIndex.Utilities.Log.Interfaces;
using Newtonsoft.Json;
using Index = MakeIndex.Models.Index;
using Version = MakeIndex.Models.Version;

namespace MakeIndex.Services;

public class RegistryService
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;
    private readonly string _registryPath;

    public RegistryService(IFileSystem fileSystem, ILogger logger, string indexDirectory)
    {
        _fileSystem = fileSystem;
        _logger = logger;
        _registryPath = Path.Combine(indexDirectory, "registry.json");
    }

    public Registry LoadRegistry()
    {
        if (!_fileSystem.FileExists(_registryPath))
        {
            _logger.Information("Registry file not found, creating new registry");
            return new Registry { Version = new Version(1, 0, 0), Entities = new List<RegistryEntity>() };
        }

        try
        {
            var json = File.ReadAllText(_registryPath);
            var registry = JsonConvert.DeserializeObject<Registry>(json);
            return registry ?? new Registry { Version = new Version(1, 0, 0), Entities = new List<RegistryEntity>() };
        }
        catch (Exception ex)
        {
            _logger.Error($"Error loading registry: {ex.Message}");
            return new Registry { Version = new Version(1, 0, 0), Entities = new List<RegistryEntity>() };
        }
    }

    public bool SaveRegistry(Registry registry)
    {
        try
        {
            var directory = Path.GetDirectoryName(_registryPath);
            if (!string.IsNullOrEmpty(directory) && !_fileSystem.DirectoryExists(directory))
            {
                _fileSystem.CreateDirectory(directory);
            }

            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Include
            };
            
            var json = JsonConvert.SerializeObject(registry, settings);
            File.WriteAllText(_registryPath, json);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error($"Error saving registry: {ex.Message}");
            return false;
        }
    }

    public void AddIndexToRegistry(Index index, string indexId, string indexPath, bool useBson = false)
    {
        var registry = LoadRegistry();
        
        var entity = new RegistryEntity
        {
            Id = indexId,
            FileName = Path.GetFileName(indexPath),
            BasicDirectory = index.MetaData.BasicDirectory,
            CreatedAt = index.MetaData.CreatedAt,
            Timestamp = index.MetaData.Timestamp,
            Binaries = useBson,
            FileCount = index.Files.Count,
            FileSize = index.Files.Values.Sum(f => f.Size),
            HashesIncluded = index.MetaData.HashesIncluded
        };

        registry.Entities.Add(entity);
        SaveRegistry(registry);
    }

    public bool RemoveIndexFromRegistry(string indexId)
    {
        var registry = LoadRegistry();
        var entity = registry.Entities.FirstOrDefault(e => e.Id == indexId);
        if (entity == null) return false;

        registry.Entities.Remove(entity);
        return SaveRegistry(registry);
    }

    public RegistryEntity? GetIndexInfo(string indexId)
    {
        var registry = LoadRegistry();
        return registry.Entities.FirstOrDefault(e => e.Id == indexId);
    }

    public List<RegistryEntity> GetAllIndices()
    {
        var registry = LoadRegistry();
        return registry.Entities.OrderByDescending(e => e.Timestamp).ToList();
    }
}
