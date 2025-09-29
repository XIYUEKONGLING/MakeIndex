using MakeIndex.Commands.Settings;
using MakeIndex.Core;
using MakeIndex.Models;
using MakeIndex.Models.Result;
using MakeIndex.Services;
using MakeIndex.Utilities.Log.Interfaces;
using Spectre.Console.Cli;

namespace MakeIndex.Commands;

public class CompareCommand : Command<CompareSettings>
{
    private readonly ILogger _logger;
    private readonly RegistryService _registryService;

    public CompareCommand(ILogger logger, RegistryService registryService)
    {
        _logger = logger;
        _registryService = registryService;
    }

    public override int Execute(CommandContext context, CompareSettings settings)
    {
        try
        {
            var index1Info = _registryService.GetIndexInfo(settings.IndexId1);
            var index2Info = _registryService.GetIndexInfo(settings.IndexId2);

            if (index1Info == null || index2Info == null)
            {
                _logger.Error($"One or both indices not found: {settings.IndexId1}, {settings.IndexId2}");
                return 1;
            }

            var indexPath1 = Path.Combine(
                Path.GetDirectoryName(_registryService.GetType().Assembly.Location) ?? ".",
                ".indexes",
                "files",
                index1Info.FileName ?? $"{settings.IndexId1}.{(index1Info.Binaries ? "bson" : "json")}"
            );

            var indexPath2 = Path.Combine(
                Path.GetDirectoryName(_registryService.GetType().Assembly.Location) ?? ".",
                ".indexes",
                "files",
                index2Info.FileName ?? $"{settings.IndexId2}.{(index2Info.Binaries ? "bson" : "json")}"
            );

            var index1 = IndexSerializer.Deserialize(indexPath1, index1Info.Binaries);
            var index2 = IndexSerializer.Deserialize(indexPath2, index2Info.Binaries);

            if (index1 == null || index2 == null)
            {
                _logger.Error("Failed to load one or both indices");
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
            _logger.Error($"Compare command failed: {ex.Message}");
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(
                CommandResult.Fail(ex.Message),
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
            ));
            return 1;
        }
    }
}