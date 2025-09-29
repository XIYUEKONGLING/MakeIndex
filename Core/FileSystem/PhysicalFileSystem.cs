using System.IO;
using MakeIndex.Interfaces;

namespace MakeIndex.Core.FileSystem;

public class PhysicalFileSystem : IFileSystem
{
    public bool DirectoryExists(string path) => Directory.Exists(path);
    public bool FileExists(string path) => File.Exists(path);
    
    public IEnumerable<string> EnumerateDirectories(string path, string pattern, SearchOption option) 
        => Directory.EnumerateDirectories(path, pattern, option);
    
    public IEnumerable<string> EnumerateFiles(string path, string pattern, SearchOption option) 
        => Directory.EnumerateFiles(path, pattern, option);
    
    public FileInfo GetFileInfo(string path) => new(path);
    public DirectoryInfo GetDirectoryInfo(string path) => new(path);
    
    public Stream OpenRead(string path) => File.OpenRead(path);
    public Stream OpenWrite(string path) => File.OpenWrite(path);
    
    public void DeleteFile(string path) => File.Delete(path);
    public void DeleteDirectory(string path, bool recursive) => Directory.Delete(path, recursive);
    public void CreateDirectory(string path) => Directory.CreateDirectory(path);
    
    public string GetFullPath(string path) => Path.GetFullPath(path);
    public string CombinePaths(params string[] paths) => Path.Combine(paths);
    public string GetDirectoryName(string path) => Path.GetDirectoryName(path) ?? string.Empty;
    public string GetFileName(string path) => Path.GetFileName(path);
    public string GetFileNameWithoutExtension(string path) => Path.GetFileNameWithoutExtension(path);
    
    public void CopyFile(string source, string destination, bool overwrite = false) 
        => File.Copy(source, destination, overwrite);
    
    public void MoveFile(string source, string destination) => File.Move(source, destination);
    
    public long GetFileSize(string path) => new FileInfo(path).Length;
    public DateTime GetLastWriteTime(string path) => File.GetLastWriteTime(path);
    public DateTime GetCreationTime(string path) => File.GetCreationTime(path);
}
