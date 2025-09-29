using System.ComponentModel;
using Spectre.Console.Cli;

namespace MakeIndex.Commands.Settings;

public class DeleteSettings : CommandSettings
{
    [CommandArgument(0, "<INDEX_ID>")]
    [Description("ID of the index to delete")]
    public string IndexId { get; set; } = string.Empty;
}