using MakeIndex.Commands.Settings;
using MakeIndex.Core.FileSystem;
using MakeIndex.Models.Result;
using MakeIndex.Services;
using MakeIndex.Utilities.Log;
using Spectre.Console.Cli;

namespace MakeIndex.Commands;

public class DeleteCommand : Command<DeleteSettings>
{
    public override int Execute(CommandContext context, DeleteSettings settings)
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
                Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(
                    CommandResult.Fail($"Index not found: {settings.IndexId}"),
                    new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
                ));
                return 1;
            }

            // Delete the index file
            var indexPath = Path.Combine(
                settings.IndexDirectory ?? ".indexes", 
                "files",
                indexInfo.FileName ?? $"{settings.IndexId}.{(indexInfo.Binaries ? "bson" : "json")}"
            );

            if (File.Exists(indexPath))
            {
                File.Delete(indexPath);
                logger.Information($"Deleted index file: {indexPath}");
            }

            // Remove from registry
            if (registryService.RemoveIndexFromRegistry(settings.IndexId))
            {
                logger.Information($"Index deleted successfully: {settings.IndexId}");
                Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(
                    CommandResult.Ok($"Index deleted successfully: {settings.IndexId}"),
                    new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
                ));
                return 0;
            }
            else
            {
                logger.Error($"Failed to remove index from registry: {settings.IndexId}");
                Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(
                    CommandResult.Fail($"Failed to remove index from registry: {settings.IndexId}"),
                    new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
                ));
                return 1;
            }
        }
        catch (Exception ex)
        {
            logger.Error($"Delete command failed: {ex.Message}");
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(
                CommandResult.Fail(ex.Message),
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
            ));
            return 1;
        }
    }
}