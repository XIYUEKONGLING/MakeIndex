using MakeIndex.Commands.Settings;
using MakeIndex.Core;
using MakeIndex.Models.Result;
using MakeIndex.Services;
using MakeIndex.Utilities.Log.Interfaces;
using Spectre.Console.Cli;

namespace MakeIndex.Commands;

public class IndexCommand : Command<IndexSettings>
{
    private readonly ILogger _logger;
    private readonly RegistryService _registryService;

    public IndexCommand(ILogger logger, RegistryService registryService)
    {
        _logger = logger;
        _registryService = registryService;
    }

    public override int Execute(CommandContext context, IndexSettings settings)
    {
        try
        {
            _logger.Information($"Indexing directory: {settings.Directory}");
            
            var indexer = new Indexer(_logger);
            var index = indexer.CreateIndex(settings.Directory, settings.CalculateHashes);
            
            if (index == null)
            {
                _logger.Error("Failed to create index");
                return 1;
            }

            var indexId = IndexIdGenerator.Generate(settings.Directory);
            var indexPath = Path.Combine(
                Path.GetDirectoryName(System.AppContext.BaseDirectory) ?? ".",
                ".indexes",
                $"files",
                $"{indexId}.{(settings.UseBson ? "bson" : "json")}"
            );

            // Ensure directory exists
            var directory = Path.GetDirectoryName(indexPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            IndexSerializer.Serialize(index, indexPath, settings.UseBson, !settings.NoIndent);
            _registryService.AddIndexToRegistry(index, indexId, indexPath, settings.UseBson);

            var result = new IndexResult
            {
                Id = indexId,
                FileCount = index.Files.Count,
                DirectoryCount = index.Directories.Count,
                TotalSize = index.Files.Values.Sum(f => f.Size),
                IndexPath = indexPath,
                CreatedAt = index.MetaData.CreatedAt
            };

            _logger.Information($"Index created successfully: {indexId}");
            _logger.Information($"Files: {result.FileCount}, Directories: {result.DirectoryCount}, Size: {FormatSize(result.TotalSize)}");

            // Output result as JSON
            var json = System.Text.Json.JsonSerializer.Serialize(
                CommandResult.Ok("Index created successfully", result),
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
            );
            Console.WriteLine(json);

            return 0;
        }
        catch (Exception ex)
        {
            _logger.Error($"Index command failed: {ex.Message}");
            var errorResult = System.Text.Json.JsonSerializer.Serialize(
                CommandResult.Fail(ex.Message),
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
            );
            Console.WriteLine(errorResult);
            return 1;
        }
    }

    private static string FormatSize(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int counter = 0;
        decimal number = bytes;
        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }
        return $"{number:n1} {suffixes[counter]}";
    }
}