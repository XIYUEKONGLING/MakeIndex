using MakeIndex.Commands.Settings;
using MakeIndex.Core;
using MakeIndex.Models.Result;
using MakeIndex.Services;
using MakeIndex.Utilities.Log.Interfaces;
using Spectre.Console.Cli;

namespace MakeIndex.Commands;

public class SizeCommand : Command<SizeSettings>
{
    private readonly ILogger _logger;
    private readonly RegistryService _registryService;

    public SizeCommand(ILogger logger, RegistryService registryService)
    {
        _logger = logger;
        _registryService = registryService;
    }

    public override int Execute(CommandContext context, SizeSettings settings)
    {
        try
        {
            var indexInfo = _registryService.GetIndexInfo(settings.IndexId);
            if (indexInfo == null)
            {
                _logger.Error($"Index not found: {settings.IndexId}");
                return 1;
            }

            var indexPath = Path.Combine(
                Path.GetDirectoryName(System.AppContext.BaseDirectory) ?? ".",
                ".indexes",
                "files",
                indexInfo.FileName ?? $"{settings.IndexId}.{(indexInfo.Binaries ? "bson" : "json")}"
            );

            var index = IndexSerializer.Deserialize(indexPath, indexInfo.Binaries);
            if (index == null)
            {
                _logger.Error($"Failed to load index: {settings.IndexId}");
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
            _logger.Error($"Size command failed: {ex.Message}");
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(
                CommandResult.Fail(ex.Message),
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
            ));
            return 1;
        }
    }
}