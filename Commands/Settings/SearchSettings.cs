using System.ComponentModel;
using Spectre.Console.Cli;

namespace MakeIndex.Commands.Settings;

public class SearchSettings : CommandSettings
{
    [CommandArgument(0, "<INDEX_ID>")]
    [Description("Index ID to search")]
    public string IndexId { get; set; } = string.Empty;

    [CommandArgument(1, "<PATTERN>")]
    [Description("Search pattern")]
    public string Pattern { get; set; } = string.Empty;

    [CommandOption("-r|--regex")]
    [Description("Use regex pattern matching")]
    public bool UseRegex { get; set; }

    [CommandOption("-c|--case-sensitive")]
    [Description("Case-sensitive search")]
    public bool CaseSensitive { get; set; }
}