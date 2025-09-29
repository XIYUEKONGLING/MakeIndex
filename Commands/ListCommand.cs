using System.ComponentModel;
using MakeIndex.Commands.Settings;
using MakeIndex.Core.FileSystem;
using MakeIndex.Models.Result;
using MakeIndex.Services;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Collections.Generic;
using System.Linq;

namespace MakeIndex.Commands;

public class ListCommand : BaseCommand<ListSettings>
{
    public class ListResult
    {
        public int TotalCount { get; set; }
        public List<IndexInfo> Indices { get; set; } = new();
        
        public class IndexInfo
        {
            public string? Id { get; set; } = string.Empty;
            public string? FileName { get; set; } = string.Empty;
            public string? BasicDirectory { get; set; } = string.Empty;
            public string? CreatedAt { get; set; } = string.Empty;
            public long Timestamp { get; set; }
            public long FileCount { get; set; }
            public long FileSize { get; set; }
            public string FileSizeHuman { get; set; } = string.Empty;
            public bool Binaries { get; set; }
            public bool HashesIncluded { get; set; }
        }
    }

    protected override int ExecuteCommand(CommandContext context, ListSettings settings)
    {
        var fileSystem = new PhysicalFileSystem();
        var registryService = new RegistryService(fileSystem, Logger, settings.IndexDirectory ?? ".indexes");
        
        try
        {
            var indices = registryService.GetAllIndices();
            
            if (!indices.Any())
            {
                Logger.Information("No indices found");
                OutputResult(new ListResult { TotalCount = 0, Indices = new() });
                return 0;
            }

            var result = new ListResult
            {
                TotalCount = indices.Count,
                Indices = indices.Select(i => new ListResult.IndexInfo
                {
                    Id = i.Id,
                    FileName = i.FileName,
                    BasicDirectory = i.BasicDirectory,
                    CreatedAt = i.CreatedAt,
                    Timestamp = i.Timestamp,
                    FileCount = i.FileCount,
                    FileSize = i.FileSize,
                    FileSizeHuman = FormatSize(i.FileSize),
                    Binaries = i.Binaries,
                    HashesIncluded = i.HashesIncluded
                }).ToList()
            };

            if (settings.Verbose)
            {
                Logger.Information($"Found {indices.Count} indices:");
                foreach (var index in indices)
                {
                    Logger.Information($"  {index.Id}: {index.BasicDirectory} ({index.FileCount} files, {FormatSize(index.FileSize)})");
                }
            }

            OutputResult(result);
            return 0;
        }
        catch (Exception ex)
        {
            Logger.Error($"List command failed: {ex.Message}");
            OutputResult(new { Error = ex.Message }, false);
            return 1;
        }
    }

    protected override void OutputHumanReadable(object result, bool success, string message)
    {
        if (result is ListResult listResult)
        {
            lock (Logger.ConsoleLock)
            {
                var table = new Table();
                table.AddColumn("ID");
                table.AddColumn("Directory");
                table.AddColumn("Files");
                table.AddColumn("Size");
                table.AddColumn("Created At");

                foreach (var index in listResult.Indices)
                {
                    table.AddRow(
                        index.Id ?? string.Empty,
                        index.BasicDirectory ?? string.Empty,
                        index.FileCount.ToString(),
                        index.FileSizeHuman,
                        index.CreatedAt ?? string.Empty
                    );
                }

                AnsiConsole.Write(table);
            }
        }
    }
}
