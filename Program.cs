using System.Reflection;
using MakeIndex.Commands;
using MakeIndex.Utilities.Log;
using Spectre.Console.Cli;

namespace MakeIndex;

public static class Program
{
    private static readonly ConsoleLogger Logger = new ConsoleLogger();
    
    public static int Main(string[] args)
    {
        var app = new CommandApp();
        app.Configure(config =>
        {
            config.SetApplicationName("MakeIndex");
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            config.SetApplicationVersion(version?.ToString() ?? "0.0.0.0");
            
            // Add commands
            config.AddCommand<IndexCommand>("index")
                .WithDescription("Create a new index of a directory");
                
            config.AddCommand<ListCommand>("list")
                .WithDescription("List all saved indices");
                
            config.AddCommand<DeleteCommand>("delete")
                .WithDescription("Delete an index");
                
            config.AddCommand<CompareCommand>("compare")
                .WithDescription("Compare two indices");
                
            config.AddCommand<SearchCommand>("search")
                .WithDescription("Search an index");
                
            config.AddCommand<SizeCommand>("size")
                .WithDescription("Analyze directory sizes");
        });
        
        return app.Run(args);
    }
}
