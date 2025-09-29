using System.ComponentModel;
using Spectre.Console.Cli;

namespace MakeIndex.Commands.Settings;

public class ListSettings : CommandSettings
{
    [CommandOption("-v|--verbose")]
    [Description("Show detailed information")]
    public bool Verbose { get; set; }
}