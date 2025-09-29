using MakeIndex.Utilities.Log;

namespace MakeIndex;

public static class Program
{
    private static readonly ConsoleLogger Logger = new ConsoleLogger();
    
    public static void Main(string[] args)
    {
        Logger.Information("Hello, World!");
    }
}