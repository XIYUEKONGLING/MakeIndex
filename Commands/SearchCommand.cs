using System.Text.RegularExpressions;
using MakeIndex.Commands.Settings;
using MakeIndex.Core;
using MakeIndex.Core.FileSystem;
using MakeIndex.Models.Result;
using MakeIndex.Services;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Collections.Generic;
using System.Linq;
using Spectre.Console.Rendering;

namespace MakeIndex.Commands;

public class SearchCommand : BaseCommand<SearchSettings>
{
    public class SearchResult
    {
        public string IndexId { get; set; } = string.Empty;
        public string Pattern { get; set; } = string.Empty;
        public List<string> Matches { get; set; } = new();
        public int FileMatches { get; set; }
        public int DirectoryMatches { get; set; }
    }

    protected override int ExecuteCommand(CommandContext context, SearchSettings settings)
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

            var matches = new List<string>();
            var pattern = settings.Pattern;

            if (settings.UseRegex)
            {
                var regex = new Regex(pattern, 
                    settings.CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
                
                matches.AddRange(index.Files.Keys.Where(path => regex.IsMatch(path)));
                matches.AddRange(index.Directories.Keys.Where(path => regex.IsMatch(path)));
            }
            else
            {
                var comparison = settings.CaseSensitive 
                    ? StringComparison.Ordinal 
                    : StringComparison.OrdinalIgnoreCase;
                
                matches.AddRange(index.Files.Keys.Where(path => path.Contains(pattern, comparison)));
                matches.AddRange(index.Directories.Keys.Where(path => path.Contains(pattern, comparison)));
            }

            var result = new SearchResult
            {
                IndexId = settings.IndexId,
                Pattern = pattern,
                Matches = matches,
                FileMatches = matches.Count(m => index.Files.ContainsKey(m)),
                DirectoryMatches = matches.Count(m => index.Directories.ContainsKey(m))
            };

            OutputResult(result);
            return 0;
        }
        catch (Exception ex)
        {
            Logger.Error($"Search command failed: {ex.Message}");
            OutputResult(new { Error = ex.Message }, false);
            return 1;
        }
    }

    protected override void OutputHumanReadable(object result, bool success, string message)
    {
        if (result is SearchResult searchResult)
        {
            lock (Logger.ConsoleLock)
            {
                // Create a list of renderable components
                var renderables = new List<IRenderable>
                {
                    new Text($"Pattern: {searchResult.Pattern}"),
                    new Text($"Files: {searchResult.FileMatches}  Directories: {searchResult.DirectoryMatches}"),
                    new Rule()
                };
                
                // Add matches as text items
                foreach (var match in searchResult.Matches.Take(20))
                {
                    renderables.Add(new Text(match));
                }
                if (searchResult.Matches.Count > 20)
                {
                    renderables.Add(new Text($"... and {searchResult.Matches.Count - 20} more"));
                }
                
                // Create the panel with all renderables
                var panel = new Panel(new Rows(renderables))
                {
                    Header = new PanelHeader($"[yellow]Search Results - {searchResult.Matches.Count} matches[/]"),
                    Border = BoxBorder.Rounded
                };
                
                AnsiConsole.Write(panel);
            }
        }
    }
}
