// SizeAnalyzer.cs (updated)
using MakeIndex.Models;
using MakeIndex.Models.Result;
using System.Collections.Generic;
using System.Linq;
using Index = MakeIndex.Models.Index;

namespace MakeIndex.Core;

public class SizeAnalyzer
{
    public SizeAnalysisResult Analyze(Index index, int depth, double minSizeMB)
    {
        var minSizeBytes = (long)(minSizeMB * 1024 * 1024);
        var result = new SizeAnalysisResult();
        result.TotalSize = index.Files.Values.Sum(f => f.Size);
        
        // Group by directory path
        var directorySizes = new Dictionary<string, long>();
        
        foreach (var file in index.Files)
        {
            var path = file.Key;
            var size = file.Value.Size;
            
            // Add to root
            directorySizes[""] = directorySizes.GetValueOrDefault("") + size;
            
            // Add to all parent directories
            var parts = path.Split(Path.DirectorySeparatorChar);
            var currentPath = "";
            
            for (int i = 0; i < parts.Length - 1; i++) // Skip last part (file name)
            {
                currentPath = currentPath == "" 
                    ? parts[i] 
                    : currentPath + Path.DirectorySeparatorChar + parts[i];
                
                directorySizes[currentPath] = directorySizes.GetValueOrDefault(currentPath) + size;
            }
        }
        
        // Convert to result items
        foreach (var (path, size) in directorySizes)
        {
            if (size < minSizeBytes) continue;
            
            var depthLevel = path.Count(c => c == Path.DirectorySeparatorChar);
            if (depthLevel > depth) continue;
            
            result.Items.Add(new SizeItem
            {
                Path = path == "" ? "<ROOT>" : path,
                Size = size,
                Percentage = (double)size / result.TotalSize * 100
            });
        }
        
        // Add files that meet the size threshold
        foreach (var file in index.Files)
        {
            if (file.Value.Size < minSizeBytes) continue;
            
            result.Items.Add(new SizeItem
            {
                Path = file.Key,
                Size = file.Value.Size,
                Percentage = (double)file.Value.Size / result.TotalSize * 100
            });
        }
        
        // Sort by size descending
        result.Items = result.Items
            .OrderByDescending(i => i.Size)
            .ToList();
            
        return result;
    }
}
