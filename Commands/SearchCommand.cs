using System.Text.RegularExpressions;
using MakeIndex.Commands.Settings;
using MakeIndex.Core;
using MakeIndex.Core.FileSystem;
using MakeIndex.Models.Result;
using MakeIndex.Services;
using MakeIndex.Utilities.Log;
using Spectre.Console.Cli;

namespace MakeIndex.Commands;

public class SearchCommand : Command<SearchSettings>
{
    public override int Execute(CommandContext context, SearchSettings settings)
    {
        var logger = new ConsoleLogger();
        var fileSystem = new PhysicalFileSystem();
        var registryService = new RegistryService(fileSystem, logger, settings.IndexDirectory ?? ".indexes");
        
        try
        {
            var indexInfo = registryService.GetIndexInfo(settings.IndexId);
            if (indexInfo == null)
            {
                logger.Error($"Index not found: {settings.IndexId}");
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
                logger.Error($"Failed to load index: {settings.IndexId}");
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
            logger.Error($"Search command failed: {ex.Message}");
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(
                CommandResult.Fail(ex.Message),
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
            ));
            return 1;
        }
    }
}