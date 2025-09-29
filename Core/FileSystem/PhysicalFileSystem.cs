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
}