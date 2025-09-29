using MakeIndex.Commands.Settings;
using MakeIndex.Core;
using MakeIndex.Core.FileSystem;
using MakeIndex.Models.Result;
using MakeIndex.Services;
using MakeIndex.Utilities.Log;
using Spectre.Console.Cli;

namespace MakeIndex.Commands;

public class IndexCommand : Command<IndexSettings>
{
    public override int Execute(CommandContext context, IndexSettings settings)
    {
        var logger = new ConsoleLogger();
        var fileSystem = new PhysicalFileSystem();
        var registryService = new RegistryService(fileSystem, logger, settings.IndexDirectory ?? ".indexes");

        try
        {
            logger.Information($"Indexing directory: {settings.Directory}");
            
            var indexer = new Indexer(logger);
            var index = indexer.CreateIndex(settings.Directory, settings.CalculateHashes);
            
            if (index == null)
            {
                logger.Error("Failed to create index");
                return 1;
            }

            var indexId = IndexIdGenerator.Generate(settings.Directory);
            var indexPath = Path.Combine(
                settings.IndexDirectory ?? ".indexes", 
                "files", 
                $"{indexId}.{(settings.UseBson ? "bson" : "json")}"
            );

            // Ensure directory exists
            var directory = Path.GetDirectoryName(indexPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            IndexSerializer.Serialize(index, indexPath, settings.UseBson, !settings.NoIndent);
            registryService.AddIndexToRegistry(index, indexId, indexPath, settings.UseBson);

            var result = new IndexResult
            {
                Id = indexId,
                FileCount = index.Files.Count,
                DirectoryCount = index.Directories.Count,
                TotalSize = index.Files.Values.Sum(f => f.Size),
                IndexPath = indexPath,
                CreatedAt = index.MetaData.CreatedAt
            };

            logger.Information($"Index created successfully: {indexId}");
            logger.Information($"Files: {result.FileCount}, Directories: {result.DirectoryCount}, Size: {FormatSize(result.TotalSize)}");

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
            logger.Error($"Index command failed: {ex.Message}");
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
