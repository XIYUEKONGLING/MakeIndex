using MakeIndex.Commands.Settings;
using MakeIndex.Core;
using MakeIndex.Core.FileSystem;
using MakeIndex.Models.Result;
using MakeIndex.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MakeIndex.Commands;

public class IndexCommand : BaseCommand<IndexSettings>
{
    protected override int ExecuteCommand(CommandContext context, IndexSettings settings)
    {
        var fileSystem = new PhysicalFileSystem();
        var registryService = new RegistryService(fileSystem, Logger, settings.IndexDirectory ?? ".indexes");

        try
        {
            Logger.Information($"Indexing directory: {settings.Directory}");
            
            var indexer = new FileSystemIndexer(fileSystem, Logger);
            var index = indexer.CreateIndex(settings.Directory, settings.CalculateHashes);
            
            if (index == null)
            {
                Logger.Error("Failed to create index");
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

            Logger.Information($"Index created successfully: {indexId}");
            Logger.Information($"Files: {result.FileCount}, Directories: {result.DirectoryCount}, Size: {FormatSize(result.TotalSize)}");

            OutputResult(result);
            return 0;
        }
        catch (Exception ex)
        {
            Logger.Error($"Index command failed: {ex.Message}");
            OutputResult(new { Error = ex.Message }, false);
            return 1;
        }
    }

    protected override void OutputHumanReadable(object result, bool success, string message)
    {
        if (result is IndexResult indexResult)
        {
            lock (Logger.ConsoleLock)
            {
                AnsiConsole.MarkupLine($"[green](+) Index created:[/] {indexResult.Id}");
                AnsiConsole.MarkupLine($"[bold]Files:[/] {indexResult.FileCount}  [bold]Directories:[/] {indexResult.DirectoryCount}  [bold]Size:[/] {FormatSize(indexResult.TotalSize)}");
            }
        }
    }
}
