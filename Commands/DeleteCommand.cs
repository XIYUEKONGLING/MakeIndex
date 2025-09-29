using MakeIndex.Commands.Settings;
using MakeIndex.Models.Result;
using MakeIndex.Services;
using MakeIndex.Utilities.Log.Interfaces;
using Spectre.Console.Cli;

namespace MakeIndex.Commands;

public class DeleteCommand : Command<DeleteSettings>
{
    private readonly ILogger _logger;
    private readonly RegistryService _registryService;

    public DeleteCommand(ILogger logger, RegistryService registryService)
    {
        _logger = logger;
        _registryService = registryService;
    }

    public override int Execute(CommandContext context, DeleteSettings settings)
    {
        try
        {
            var indexInfo = _registryService.GetIndexInfo(settings.IndexId);
            if (indexInfo == null)
            {
                _logger.Error($"Index not found: {settings.IndexId}");
                Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(
                    CommandResult.Fail($"Index not found: {settings.IndexId}"),
                    new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
                ));
                return 1;
            }

            // Delete the index file
            var indexPath = Path.Combine(
                Path.GetDirectoryName(System.AppContext.BaseDirectory) ?? ".",
                ".indexes",
                "files",
                indexInfo.FileName ?? $"{settings.IndexId}.{(indexInfo.Binaries ? "bson" : "json")}"
            );

            if (File.Exists(indexPath))
            {
                File.Delete(indexPath);
                _logger.Information($"Deleted index file: {indexPath}");
            }

            // Remove from registry
            if (_registryService.RemoveIndexFromRegistry(settings.IndexId))
            {
                _logger.Information($"Index deleted successfully: {settings.IndexId}");
                Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(
                    CommandResult.Ok($"Index deleted successfully: {settings.IndexId}"),
                    new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
                ));
                return 0;
            }
            else
            {
                _logger.Error($"Failed to remove index from registry: {settings.IndexId}");
                Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(
                    CommandResult.Fail($"Failed to remove index from registry: {settings.IndexId}"),
                    new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
                ));
                return 1;
            }
        }
        catch (Exception ex)
        {
            _logger.Error($"Delete command failed: {ex.Message}");
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(
                CommandResult.Fail(ex.Message),
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
            ));
            return 1;
        }
    }
}
