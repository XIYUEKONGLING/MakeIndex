using MakeIndex.Utilities.Log;
using Spectre.Console.Cli;

namespace MakeIndex;

public static class Program
{
    private static readonly ConsoleLogger Logger = new ConsoleLogger();
    
    // public static void Main(string[] args)
    // {
    //     Logger.Information("Hello, World!");
    // }
    
    public static int Main(string[] args)
    {
        var app = new CommandApp();
        
        
        return app.Run(args);
    }
}