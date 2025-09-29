using MakeIndex.Commands.Settings;
using MakeIndex.Core;
using MakeIndex.Core.FileSystem;
using MakeIndex.Models.Result;
using MakeIndex.Services;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Collections.Generic;

namespace MakeIndex.Commands;

public class CompareCommand : BaseCommand<CompareSettings>
{
    protected override int ExecuteCommand(CommandContext context, CompareSettings settings)
    {
        var fileSystem = new PhysicalFileSystem();
        var registryService = new RegistryService(fileSystem, Logger, settings.IndexDirectory ?? ".indexes");
        
        try
        {
            var index1Info = registryService.GetIndexInfo(settings.IndexId1);
            var index2Info = registryService.GetIndexInfo(settings.IndexId2);

            if (index1Info == null || index2Info == null)
            {
                Logger.Error($"One or both indices not found: {settings.IndexId1}, {settings.IndexId2}");
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
                Logger.Error("Failed to load one or both indices");
                return 1;
            }

            var comparer = new IndexComparer();
            var result = comparer.Compare(index1, index2, settings.UseHashes);

            OutputResult(result);
            return 0;
        }
        catch (Exception ex)
        {
            Logger.Error($"Compare command failed: {ex.Message}");
            OutputResult(new { Error = ex.Message }, false);
            return 1;
        }
    }

    protected override void OutputHumanReadable(object result, bool success, string message)
    {
        if (result is CompareResult compareResult)
        {
            lock (Logger.ConsoleLock)
            {
                var table = new Table();
                table.AddColumn("Change Type");
                table.AddColumn("Count");
                table.AddColumn("Examples");

                AddComparisonRow(table, "New Files", compareResult.NewFiles);
                AddComparisonRow(table, "Modified Files", compareResult.ModifiedFiles);
                AddComparisonRow(table, "Deleted Files", compareResult.DeletedFiles);
                AddComparisonRow(table, "New Directories", compareResult.NewDirectories);
                AddComparisonRow(table, "Deleted Directories", compareResult.DeletedDirectories);

                AnsiConsole.Write(table);
            }
        }
    }

    private void AddComparisonRow(Table table, string title, List<string> items)
    {
        var count = items.Count;
        var examples = count > 0 && !((CompareSettings)Settings).FullOutput
            ? string.Join("\n", items.Take(5)) + (count > 5 ? $"\n... and {count - 5} more" : "")
            : string.Join("\n", items);

        table.AddRow(
            title,
            count.ToString(),
            examples
        );
    }
}
