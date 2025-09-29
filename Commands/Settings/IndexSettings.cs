using System.ComponentModel;
using Spectre.Console.Cli;

namespace MakeIndex.Commands.Settings;

public class IndexSettings : GlobalSettings
{
    [CommandArgument(0, "<DIRECTORY>")]
    [Description("Directory to index")]
    public string Directory { get; set; } = string.Empty;
    
    [CommandOption("-h|--hashes")]
    [Description("Calculate and store file hashes")]
    public bool CalculateHashes { get; set; }
    
    [CommandOption("-b|--bson")]
    [Description("Use BSON format instead of JSON")]
    public bool UseBson { get; set; }
    
    [CommandOption("--no-indent")]
    [Description("Disable JSON/BSON indentation")]
    public bool NoIndent { get; set; }
}