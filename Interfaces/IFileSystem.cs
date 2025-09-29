namespace MakeIndex.Interfaces;

public interface IFileSystem
{
    bool DirectoryExists(string path);
    bool FileExists(string path);
    IEnumerable<string> EnumerateDirectories(string path, string pattern, SearchOption option);
    IEnumerable<string> EnumerateFiles(string path, string pattern, SearchOption option);
    FileInfo GetFileInfo(string path);
    DirectoryInfo GetDirectoryInfo(string path);
    Stream OpenRead(string path);
}
