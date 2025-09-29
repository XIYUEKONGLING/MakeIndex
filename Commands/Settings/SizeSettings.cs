using System.ComponentModel;
using Spectre.Console.Cli;

namespace MakeIndex.Commands.Settings;

public class SizeSettings : GlobalSettings
{
    [CommandArgument(0, "<INDEX_ID>")]
    [Description("Index ID to analyze")]
    public string IndexId { get; set; } = string.Empty;
    
    [CommandOption("-d|--depth")]
    [Description("Directory depth to display")]
    public int Depth { get; set; } = 2;
    
    [CommandOption("-m|--min-size")]
    [Description("Minimum size in MB to display")]
    public double MinSizeMB { get; set; } = 0;
    
    [CommandOption("--full")]
    [Description("Show all size details")]
    public bool FullOutput { get; set; }
}