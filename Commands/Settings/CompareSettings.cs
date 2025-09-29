using System.ComponentModel;
using Spectre.Console.Cli;

namespace MakeIndex.Commands.Settings;

public class CompareSettings : GlobalSettings
{
    [CommandArgument(0, "<INDEX_ID1>")]
    [Description("First index ID")]
    public string IndexId1 { get; set; } = string.Empty;
    
    [CommandArgument(1, "<INDEX_ID2>")]
    [Description("Second index ID")]
    public string IndexId2 { get; set; } = string.Empty;
    
    [CommandOption("-h|--hashes")]
    [Description("Use hashes for comparison if available")]
    public bool UseHashes { get; set; }
}
