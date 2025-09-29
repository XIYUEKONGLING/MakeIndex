using System.ComponentModel;
using MakeIndex.Commands.Settings;
using MakeIndex.Core.FileSystem;
using MakeIndex.Models.Result;
using MakeIndex.Services;
using MakeIndex.Utilities.Log;
using Spectre.Console.Cli;

namespace MakeIndex.Commands;

public class ListCommand : Command<ListSettings>
{
    public override int Execute(CommandContext context, ListSettings settings)
    {
        var logger = new ConsoleLogger();
        var fileSystem = new PhysicalFileSystem();
        var registryService = new RegistryService(fileSystem, logger, settings.IndexDirectory ?? ".indexes");
        
        try
        {
            var indices = registryService.GetAllIndices();
            
            if (!indices.Any())
            {
                logger.Information("No indices found");
                Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(
                    CommandResult.Ok("No indices found", new { Indices = Array.Empty<object>() }),
                    new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
                ));
                return 0;
            }

            var result = new
            {
                TotalCount = indices.Count,
                Indices = indices.Select(i => new
                {
                    i.Id,
                    i.FileName,
                    i.BasicDirectory,
                    i.CreatedAt,
                    i.Timestamp,
                    i.FileCount,
                    FileSize = i.FileSize,
                    FileSizeHuman = FormatSize(i.FileSize),
                    i.Binaries,
                    i.HashesIncluded
                })
            };

            if (settings.Verbose)
            {
                logger.Information($"Found {indices.Count} indices:");
                foreach (var index in indices)
                {
                    logger.Information($"  {index.Id}: {index.BasicDirectory} ({index.FileCount} files, {FormatSize(index.FileSize)})");
                }
            }

            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(
                CommandResult.Ok($"Found {indices.Count} indices", result),
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
            ));

            return 0;
        }
        catch (Exception ex)
        {
            logger.Error($"List command failed: {ex.Message}");
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(
                CommandResult.Fail(ex.Message),
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
            ));
            return 1;
        }
    }

    private static string FormatSize(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int counter = 0;
        decimal number = bytes;
        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }
        return $"{number:n1} {suffixes[counter]}";
    }
}