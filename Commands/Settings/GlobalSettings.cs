using System.ComponentModel;
using Spectre.Console.Cli;

namespace MakeIndex.Commands.Settings;

public class GlobalSettings : CommandSettings
{
    [CommandOption("-i|--index-dir")]
    [Description("Directory to store index files")]
    public string? IndexDirectory { get; set; } = ".indexes";
    
    [CommandOption("-v|--verbose")]
    [Description("Enable verbose logging")]
    public bool Verbose { get; set; }
    
    [CommandOption("--no-color")]
    [Description("Disable colored output")]
    public bool NoColor { get; set; }
    
    [CommandOption("--json")]
    [Description("Output in JSON format")]
    public bool JsonOutput { get; set; }
}