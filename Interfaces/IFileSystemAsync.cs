namespace MakeIndex.Interfaces;

public interface IFileSystemAsync : IFileSystem
{
    Task<bool> DirectoryExistsAsync(string path);
    Task<bool> FileExistsAsync(string path);
    Task<IEnumerable<string>> EnumerateDirectoriesAsync(string path, string pattern, SearchOption option);
    Task<IEnumerable<string>> EnumerateFilesAsync(string path, string pattern, SearchOption option);
    Task<FileInfo> GetFileInfoAsync(string path);
    Task<DirectoryInfo> GetDirectoryInfoAsync(string path);
    Task<Stream> OpenReadAsync(string path);
    Task<long> GetFileSizeAsync(string path);
    Task<DateTime> GetLastWriteTimeAsync(string path);
}
