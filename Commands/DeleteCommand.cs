using MakeIndex.Commands.Settings;
using MakeIndex.Core.FileSystem;
using MakeIndex.Models.Result;
using MakeIndex.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MakeIndex.Commands;

public class DeleteCommand : BaseCommand<DeleteSettings>
{
    public class DeleteResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? Error { get; set; }
    }

    protected override int ExecuteCommand(CommandContext context, DeleteSettings settings)
    {
        var fileSystem = new PhysicalFileSystem();
        var registryService = new RegistryService(fileSystem, Logger, settings.IndexDirectory ?? ".indexes");
        
        try
        {
            var indexInfo = registryService.GetIndexInfo(settings.IndexId);
            if (indexInfo == null)
            {
                Logger.Error($"Index not found: {settings.IndexId}");
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
                Logger.Information($"Deleted index file: {indexPath}");
            }

            // Remove from registry
            if (registryService.RemoveIndexFromRegistry(settings.IndexId))
            {
                Logger.Information($"Index deleted successfully: {settings.IndexId}");
                OutputResult(new DeleteResult { Success = true, Message = $"Index deleted: {settings.IndexId}" });
                return 0;
            }
            else
            {
                Logger.Error($"Failed to remove index from registry: {settings.IndexId}");
                OutputResult(new DeleteResult { Success = false, Error = $"Failed to remove index from registry: {settings.IndexId}" });
                return 1;
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Delete command failed: {ex.Message}");
            OutputResult(new DeleteResult { Success = false, Error = ex.Message });
            return 1;
        }
    }

    protected override void OutputHumanReadable(object result, bool success, string message)
    {
        if (result is DeleteResult deleteResult && deleteResult.Success)
        {
            lock (Logger.ConsoleLock)
            {
                AnsiConsole.MarkupLine($"[green]✓ {deleteResult.Message}[/]");
            }
        }
        else if (result is DeleteResult errorResult && !errorResult.Success)
        {
            lock (Logger.ConsoleLock)
            {
                AnsiConsole.MarkupLine($"[red]✗ {errorResult.Error}[/]");
            }
        }
    }
}
