namespace MakeIndex.Interfaces;

public interface IFileSystem
{
    // Basic operations
    bool DirectoryExists(string path);
    bool FileExists(string path);
    
    // Enumeration
    IEnumerable<string> EnumerateDirectories(string path, string pattern, SearchOption option);
    IEnumerable<string> EnumerateFiles(string path, string pattern, SearchOption option);
    
    // File operations
    FileInfo GetFileInfo(string path);
    DirectoryInfo GetDirectoryInfo(string path);
    Stream OpenRead(string path);
    Stream OpenWrite(string path);
    void DeleteFile(string path);
    void DeleteDirectory(string path, bool recursive);
    void CreateDirectory(string path);
    
    // Path operations
    string GetFullPath(string path);
    string CombinePaths(params string[] paths);
    string GetDirectoryName(string path);
    string GetFileName(string path);
    string GetFileNameWithoutExtension(string path);
    
    // Utility
    void CopyFile(string source, string destination, bool overwrite = false);
    void MoveFile(string source, string destination);
    long GetFileSize(string path);
    DateTime GetLastWriteTime(string path);
    DateTime GetCreationTime(string path);
}