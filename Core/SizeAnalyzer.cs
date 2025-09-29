using MakeIndex.Models;
using MakeIndex.Models.Result;
using MakeIndex.Utilities;
using Index = MakeIndex.Models.Index;

namespace MakeIndex.Core;

public class SizeAnalyzer
{
    public object Analyze(Index index, int depth, double minSizeMB)
    {
        var minSizeBytes = (long)(minSizeMB * 1024 * 1024);
        var directorySizes = new Dictionary<string, long>();
        var fileSizes = new List<FileSizeResult>();

        // Calculate directory sizes
        foreach (var file in index.Files)
        {
            var dir = Path.GetDirectoryName(file.Key) ?? string.Empty;
            var parts = dir.Split(Path.DirectorySeparatorChar);
            var current = string.Empty;

            foreach (var part in parts)
            {
                current = string.IsNullOrEmpty(current) ? part : $"{current}{Path.DirectorySeparatorChar}{part}";
                
                if (directorySizes.ContainsKey(current))
                {
                    directorySizes[current] += file.Value.Size;
                }
                else
                {
                    directorySizes[current] = file.Value.Size;
                }
            }

            // Add file to results if it meets size threshold
            if (file.Value.Size >= minSizeBytes)
            {
                fileSizes.Add(new FileSizeResult
                {
                    Path = file.Key,
                    Size = file.Value.Size,
                    SizeHuman = FormatSize(file.Value.Size)
                });
            }
        }

        // Prepare directory results
        var dirResults = directorySizes
            .Where(kv => kv.Value >= minSizeBytes && 
                         kv.Key.Count(c => c == Path.DirectorySeparatorChar) < depth)
            .Select(kv => new DirectorySizeResult
            {
                Path = kv.Key,
                Size = kv.Value,
                SizeHuman = FormatSize(kv.Value),
                Depth = kv.Key.Count(c => c == Path.DirectorySeparatorChar)
            })
            .OrderByDescending(d => d.Size)
            .ToList();

        return new
        {
            TotalSize = index.Files.Values.Sum(f => f.Size),
            TotalSizeHuman = FormatSize(index.Files.Values.Sum(f => f.Size)),
            Directories = dirResults,
            Files = fileSizes.OrderByDescending(f => f.Size).ToList()
        };
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