using MakeIndex.Models;
using MakeIndex.Models.Result;
using Index = MakeIndex.Models.Index;

namespace MakeIndex.Core;

public class IndexComparer
{
    public CompareResult Compare(Index index1, Index index2, bool useHashes)
    {
        var result = new CompareResult
        {
            HashVerificationPerformed = useHashes && 
                                        index1.MetaData.HashesIncluded && 
                                        index2.MetaData.HashesIncluded
        };

        // Compare files
        foreach (var file in index1.Files)
        {
            if (!index2.Files.ContainsKey(file.Key))
            {
                result.DeletedFiles.Add(file.Key);
            }
            else if (IsFileModified(file.Value, index2.Files[file.Key], useHashes))
            {
                result.ModifiedFiles.Add(file.Key);
            }
        }

        foreach (var file in index2.Files)
        {
            if (!index1.Files.ContainsKey(file.Key))
            {
                result.NewFiles.Add(file.Key);
            }
        }

        // Compare directories
        foreach (var dir in index1.Directories)
        {
            if (!index2.Directories.ContainsKey(dir.Key))
            {
                result.DeletedDirectories.Add(dir.Key);
            }
        }

        foreach (var dir in index2.Directories)
        {
            if (!index1.Directories.ContainsKey(dir.Key))
            {
                result.NewDirectories.Add(dir.Key);
            }
        }

        // Create summary
        result.Summary = new CompareSummary
        {
            FilesAdded = result.NewFiles.Count,
            FilesModified = result.ModifiedFiles.Count,
            FilesDeleted = result.DeletedFiles.Count,
            DirectoriesAdded = result.NewDirectories.Count,
            DirectoriesDeleted = result.DeletedDirectories.Count,
            TotalChanges = result.NewFiles.Count + result.ModifiedFiles.Count + 
                          result.DeletedFiles.Count + result.NewDirectories.Count + 
                          result.DeletedDirectories.Count
        };

        return result;
    }

    private bool IsFileModified(IndexFile file1, IndexFile file2, bool useHashes)
    {
        if (file1.Size != file2.Size) return true;
        if (file1.ModifiedTime != file2.ModifiedTime) return true;
        
        if (!useHashes || file1.Hashes == null || file2.Hashes == null) 
            return false;

        foreach (var hash in file1.Hashes)
        {
            if (file2.Hashes.TryGetValue(hash.Key, out var value) && 
                hash.Value != value)
            {
                return true;
            }
        }

        return false;
    }
}
