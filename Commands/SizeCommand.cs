using MakeIndex.Commands.Settings;
using MakeIndex.Core;
using MakeIndex.Core.FileSystem;
using MakeIndex.Models.Result;
using MakeIndex.Services;
using MakeIndex.Utilities.Log;
using Spectre.Console.Cli;

namespace MakeIndex.Commands;

public class SizeCommand : Command<SizeSettings>
{
    public override int Execute(CommandContext context, SizeSettings settings)
    {
        var logger = new ConsoleLogger();
        var fileSystem = new PhysicalFileSystem();
        var registryService = new RegistryService(fileSystem, logger, settings.IndexDirectory ?? ".indexes");
        
        try
        {
            var indexInfo = registryService.GetIndexInfo(settings.IndexId);
            if (indexInfo == null)
            {
                logger.Error($"Index not found: {settings.IndexId}");
                return 1;
            }

            var indexPath = Path.Combine(
                settings.IndexDirectory ?? ".indexes", 
                "files",
                indexInfo.FileName ?? $"{settings.IndexId}.{(indexInfo.Binaries ? "bson" : "json")}"
            );

            var index = IndexSerializer.Deserialize(indexPath, indexInfo.Binaries);
            if (index == null)
            {
                logger.Error($"Failed to load index: {settings.IndexId}");
                return 1;
            }

            var sizeAnalyzer = new SizeAnalyzer();
            var result = sizeAnalyzer.Analyze(index, settings.Depth, settings.MinSizeMB);

            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(
                CommandResult.Ok("Size analysis completed", result),
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
            ));

            return 0;
        }
        catch (Exception ex)
        {
            logger.Error($"Size command failed: {ex.Message}");
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(
                CommandResult.Fail(ex.Message),
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
            ));
            return 1;
        }
    }
}