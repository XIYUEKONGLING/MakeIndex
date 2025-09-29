// BaseCommand.cs
using MakeIndex.Commands.Settings;
using MakeIndex.Utilities.Log;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MakeIndex.Commands;

public abstract class BaseCommand<T> : Command<T> where T : GlobalSettings
{
    protected ConsoleLogger Logger { get; private set; } = null!;
    protected GlobalSettings Settings { get; private set; } = null!;

    public override int Execute(CommandContext context, T settings)
    {
        Settings = settings;
        Logger = new ConsoleLogger(!settings.NoColor);
        return ExecuteCommand(context, settings);
    }

    protected abstract int ExecuteCommand(CommandContext context, T settings);

    protected void OutputResult(object result, bool success = true, string message = "")
    {
        if (Settings.JsonOutput)
        {
            OutputJson(result);
        }
        else
        {
            OutputHumanReadable(result, success, message);
        }
    }

    protected virtual void OutputJson(object result)
    {
        lock (Logger.ConsoleLock)
        {
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(
                result,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
            ));
        }
    }

    protected virtual void OutputHumanReadable(object result, bool success, string message)
    {
        // To be implemented in each command
    }
    
    protected static string FormatSize(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int counter = 0;
        decimal number = bytes;
        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }
        return $"{number:n1} {suffixes[counter]}";
    }
}