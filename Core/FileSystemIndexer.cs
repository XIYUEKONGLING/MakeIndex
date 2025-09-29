using System.Collections.Concurrent;
using System.Security.Cryptography;
using MakeIndex.Interfaces;
using MakeIndex.Models;
using MakeIndex.Utilities;
using MakeIndex.Utilities.Log.Interfaces;
using Index = MakeIndex.Models.Index;
using Version = MakeIndex.Models.Version;

namespace MakeIndex.Core;

public sealed class FileSystemIndexer
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;
    private readonly int _bufferSize;
    private static readonly Version CurrentIndexVersion = new(1, 0, 0);

    public FileSystemIndexer(IFileSystem fileSystem, ILogger logger, int bufferSize = 8192)
    {
        _fileSystem = fileSystem;
        _logger = logger;
        _bufferSize = bufferSize > 0 ? bufferSize : 8192;
    }

    public Index? CreateIndex(string baseDirectory, bool calculateHashes = false)
    {
        var basePath = _fileSystem.GetFullPath(baseDirectory);
        if (!_fileSystem.DirectoryExists(basePath))
        {
            _logger.Error($"Directory does not exist: {basePath}");
            return null;
        }

        var index = new Index
        {
            Version = CurrentIndexVersion,
            MetaData = new IndexMetaData
            {
                BasicDirectory = basePath,
                CreatedAt = TimeUtils.GetUtcIsoString(),
                Timestamp = TimeUtils.GetUnixTimestampMilliseconds(),
                HashesIncluded = calculateHashes
            }
        };

        _logger.Information($"Indexing directory: {basePath}");
        var startTime = DateTime.UtcNow;

        try
        {
            // Process directories
            var directories = _fileSystem.EnumerateDirectories(basePath, "*", SearchOption.AllDirectories)
                .Append(basePath)
                .ToList();

            _logger.Information($"Found {directories.Count} directories");
            foreach (var directory in directories)
            {
                ProcessDirectory(basePath, directory, index.Directories);
            }

            // Process files
            var files = _fileSystem.EnumerateFiles(basePath, "*", SearchOption.AllDirectories).ToList();
            _logger.Information($"Found {files.Count} files");

            var fileData = new ConcurrentDictionary<string, IndexFile>();
            Parallel.ForEach(files, file => 
            {
                ProcessFile(basePath, file, fileData);
            });

            // Calculate hashes in parallel if requested
            if (calculateHashes)
            {
                _logger.Information($"Calculating hashes for {files.Count} files...");
                var hashStart = DateTime.UtcNow;
                
                Parallel.ForEach(Partitioner.Create(files, EnumerablePartitionerOptions.NoBuffering), 
                    new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                    file => CalculateFileHashes(file, fileData));
                
                _logger.Information($"Hash calculation completed in {(DateTime.UtcNow - hashStart).TotalSeconds:F2} seconds");
            }

            // Add processed files to index
            foreach (var (key, value) in fileData)
            {
                index.Files[key] = value;
            }
        }
        catch (Exception ex)
        {
            _logger.Error($"Indexing failed: {ex.Message}");
            return null;
        }

        var elapsed = DateTime.UtcNow - startTime;
        _logger.Information($"Indexing completed in {elapsed.TotalSeconds:F2} seconds");
        return index;
    }

    private void ProcessDirectory(string basePath, string directoryPath, IDictionary<string, IndexDirectory> directories)
    {
        try
        {
            var dirInfo = _fileSystem.GetDirectoryInfo(directoryPath);
            var relativePath = GetRelativePath(basePath, directoryPath);

            directories[relativePath] = new IndexDirectory
            {
                Path = relativePath,
                ModifiedTime = dirInfo.LastWriteTimeUtc.ToUnixTimestampMilliseconds(),
                CreatedTime = dirInfo.CreationTimeUtc.ToUnixTimestampMilliseconds()
            };
        }
        catch (Exception ex)
        {
            _logger.Warning($"Error processing directory {directoryPath}: {ex.Message}");
        }
    }

    private void ProcessFile(string basePath, string filePath, ConcurrentDictionary<string, IndexFile> fileData)
    {
        try
        {
            var fileInfo = _fileSystem.GetFileInfo(filePath);
            var relativePath = GetRelativePath(basePath, filePath);

            fileData[relativePath] = new IndexFile
            {
                Path = relativePath,
                Size = fileInfo.Length,
                ModifiedTime = fileInfo.LastWriteTimeUtc.ToUnixTimestampMilliseconds(),
                CreatedTime = fileInfo.CreationTimeUtc.ToUnixTimestampMilliseconds(),
                Type = fileInfo.Extension.TrimStart('.').ToLowerInvariant()
            };
        }
        catch (Exception ex)
        {
            _logger.Warning($"Error processing file {filePath}: {ex.Message}");
        }
    }

    private void CalculateFileHashes(string filePath, ConcurrentDictionary<string, IndexFile> fileData)
    {
        try
        {
            var relativePath = GetRelativePathFromFileData(filePath, fileData);
            if (string.IsNullOrEmpty(relativePath)) return;

            using var stream = _fileSystem.OpenRead(filePath);
            var hashes = ComputeAllHashes(stream);

            if (fileData.TryGetValue(relativePath, out var file))
            {
                file.Hashes = hashes;
            }
        }
        catch (Exception ex)
        {
            _logger.Warning($"Error calculating hashes for {filePath}: {ex.Message}");
        }
    }

    private string GetRelativePath(string basePath, string fullPath)
    {
        // Simple relative path calculation
        if (!fullPath.StartsWith(basePath))
            return fullPath;

        var relative = fullPath.Substring(basePath.Length).TrimStart(Path.DirectorySeparatorChar);
        return string.IsNullOrEmpty(relative) ? string.Empty : relative;
    }

    private static string? GetRelativePathFromFileData(string filePath, ConcurrentDictionary<string, IndexFile> fileData)
    {
        // Find the key that matches the file path
        return fileData.Keys.FirstOrDefault(key => 
            filePath.EndsWith(key, StringComparison.OrdinalIgnoreCase));
    }

    #region Hashes

    private Dictionary<string, string> ComputeAllHashes(Stream stream)
    {
        // Create all hash algorithms
        using var md5 = MD5.Create();
        using var sha1 = SHA1.Create();
        using var sha256 = SHA256.Create();
        using var sha384 = SHA384.Create();
        using var sha512 = SHA512.Create();
        using var sha3256 = SHA3_256.Create();
        using var sha3384 = SHA3_384.Create();
        using var sha3512 = SHA3_512.Create();

        // Read file once and update all hashes
        var buffer = new byte[_bufferSize];
        int bytesRead;
        while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
        {
            md5.TransformBlock(buffer, 0, bytesRead, null, 0);
            sha1.TransformBlock(buffer, 0, bytesRead, null, 0);
            sha256.TransformBlock(buffer, 0, bytesRead, null, 0);
            sha384.TransformBlock(buffer, 0, bytesRead, null, 0);
            sha512.TransformBlock(buffer, 0, bytesRead, null, 0);
            sha3256.TransformBlock(buffer, 0, bytesRead, null, 0);
            sha3384.TransformBlock(buffer, 0, bytesRead, null, 0);
            sha3512.TransformBlock(buffer, 0, bytesRead, null, 0);
        }

        // Finalize all hashes
        md5.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
        sha1.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
        sha256.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
        sha384.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
        sha512.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
        sha3256.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
        sha3384.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
        sha3512.TransformFinalBlock(Array.Empty<byte>(), 0, 0);

        // Return results
        return new Dictionary<string, string>
        {
            ["MD5"] = BitConverter.ToString(md5.Hash!).Replace("-", "").ToLowerInvariant(),
            ["SHA1"] = BitConverter.ToString(sha1.Hash!).Replace("-", "").ToLowerInvariant(),
            ["SHA256"] = BitConverter.ToString(sha256.Hash!).Replace("-", "").ToLowerInvariant(),
            ["SHA384"] = BitConverter.ToString(sha384.Hash!).Replace("-", "").ToLowerInvariant(),
            ["SHA512"] = BitConverter.ToString(sha512.Hash!).Replace("-", "").ToLowerInvariant(),
            ["SHA3_256"] = BitConverter.ToString(sha3256.Hash!).Replace("-", "").ToLowerInvariant(),
            ["SHA3_384"] = BitConverter.ToString(sha3384.Hash!).Replace("-", "").ToLowerInvariant(),
            ["SHA3_512"] = BitConverter.ToString(sha3512.Hash!).Replace("-", "").ToLowerInvariant()
        };
    }

    #endregion
}
