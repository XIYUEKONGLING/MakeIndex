using System.Text.RegularExpressions;
using MakeIndex.Commands.Settings;
using MakeIndex.Core;
using MakeIndex.Models.Result;
using MakeIndex.Services;
using MakeIndex.Utilities.Log.Interfaces;
using Spectre.Console.Cli;

namespace MakeIndex.Commands;

public class SearchCommand : Command<SearchSettings>
{
    private readonly ILogger _logger;
    private readonly RegistryService _registryService;

    public SearchCommand(ILogger logger, RegistryService registryService)
    {
        _logger = logger;
        _registryService = registryService;
    }

    public override int Execute(CommandContext context, SearchSettings settings)
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

            var result = new
            {
                IndexId = settings.IndexId,
                Pattern = pattern,
                Matches = matches,
                FileMatches = matches.Count(m => index.Files.ContainsKey(m)),
                DirectoryMatches = matches.Count(m => index.Directories.ContainsKey(m))
            };

            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(
                CommandResult.Ok($"Found {matches.Count} matches", result),
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
            ));

            return 0;
        }
        catch (Exception ex)
        {
            _logger.Error($"Search command failed: {ex.Message}");
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(
                CommandResult.Fail(ex.Message),
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
            ));
            return 1;
        }
    }
}