using System.Security.Cryptography;
using System.Text;

namespace MakeIndex.Core;

public static class IndexIdGenerator
{
    public static string Generate(string baseDirectory)
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(baseDirectory));
        // var random = Random.Shared.Next();
        return $"{timestamp}_{BitConverter.ToString(hash)[..8].Replace("-", "").ToLower()}";
    }
}