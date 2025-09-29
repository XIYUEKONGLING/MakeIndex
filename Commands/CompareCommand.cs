using MakeIndex.Commands.Settings;
using MakeIndex.Core;
using MakeIndex.Core.FileSystem;
using MakeIndex.Models;
using MakeIndex.Models.Result;
using MakeIndex.Services;
using MakeIndex.Utilities.Log;
using Spectre.Console.Cli;

namespace MakeIndex.Commands;

public class CompareCommand : Command<CompareSettings>
{
    public override int Execute(CommandContext context, CompareSettings settings)
    {
        var logger = new ConsoleLogger();
        var fileSystem = new PhysicalFileSystem();
        var registryService = new RegistryService(fileSystem, logger, settings.IndexDirectory ?? ".indexes");
        
        try
        {
            var index1Info = registryService.GetIndexInfo(settings.IndexId1);
            var index2Info = registryService.GetIndexInfo(settings.IndexId2);

            if (index1Info == null || index2Info == null)
            {
                logger.Error($"One or both indices not found: {settings.IndexId1}, {settings.IndexId2}");
                return 1;
            }

            var indexPath1 = Path.Combine(
                settings.IndexDirectory ?? ".indexes", 
                "files",
                index1Info.FileName ?? $"{settings.IndexId1}.{(index1Info.Binaries ? "bson" : "json")}"
            );

            var indexPath2 = Path.Combine(
                settings.IndexDirectory ?? ".indexes", 
                "files",
                index2Info.FileName ?? $"{settings.IndexId2}.{(index2Info.Binaries ? "bson" : "json")}"
            );

            var index1 = IndexSerializer.Deserialize(indexPath1, index1Info.Binaries);
            var index2 = IndexSerializer.Deserialize(indexPath2, index2Info.Binaries);

            if (index1 == null || index2 == null)
            {
                logger.Error("Failed to load one or both indices");
                return 1;
            }

            var comparer = new IndexComparer();
            var result = comparer.Compare(index1, index2, settings.UseHashes);

            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(
                CommandResult.Ok("Comparison completed", result),
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
            ));

            return 0;
        }
        catch (Exception ex)
        {
            logger.Error($"Compare command failed: {ex.Message}");
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(
                CommandResult.Fail(ex.Message),
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
            ));
            return 1;
        }
    }
}