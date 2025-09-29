using MakeIndex.Commands.Settings;
using MakeIndex.Core;
using MakeIndex.Core.FileSystem;
using MakeIndex.Models.Result;
using MakeIndex.Services;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Collections.Generic;
using System.Linq;

namespace MakeIndex.Commands;

public class SizeCommand : BaseCommand<SizeSettings>
{
    protected override int ExecuteCommand(CommandContext context, SizeSettings settings)
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

            var indexPath = Path.Combine(
                settings.IndexDirectory ?? ".indexes", 
                "files",
                indexInfo.FileName ?? $"{settings.IndexId}.{(indexInfo.Binaries ? "bson" : "json")}"
            );

            var index = IndexSerializer.Deserialize(indexPath, indexInfo.Binaries);
            if (index == null)
            {
                Logger.Error($"Failed to load index: {settings.IndexId}");
                return 1;
            }

            var sizeAnalyzer = new SizeAnalyzer();
            var result = sizeAnalyzer.Analyze(index, settings.Depth, settings.MinSizeMB);

            OutputResult(result);
            return 0;
        }
        catch (Exception ex)
        {
            Logger.Error($"Size command failed: {ex.Message}");
            OutputResult(new { Error = ex.Message }, false);
            return 1;
        }
    }

    protected override void OutputHumanReadable(object result, bool success, string message)
    {
        if (result is SizeAnalysisResult sizeResult)
        {
            lock (Logger.ConsoleLock)
            {
                var table = new Table();
                table.AddColumn("Path");
                table.AddColumn("Size");
                table.AddColumn("Percentage");

                foreach (var item in ((SizeSettings)Settings).FullOutput 
                    ? sizeResult.Items 
                    : sizeResult.Items.Take(20))
                {
                    table.AddRow(
                        item.Path,
                        FormatSize(item.Size),
                        $"{item.Percentage:F1}%"
                    );
                }

                if (!((SizeSettings)Settings).FullOutput && sizeResult.Items.Count > 20)
                {
                    table.AddRow("...", "...", "...");
                    table.AddRow($"{sizeResult.Items.Count - 20} more items", "", "");
                }

                AnsiConsole.Write(table);
            }
        }
    }
}
