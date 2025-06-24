using JetBrains.Annotations;
using McMaster.Extensions.CommandLineUtils;

namespace OfficeRibbonXEditor.CommandLine.Commands;

public abstract class BaseCommand
{
    private readonly IConsole _console;

    protected BaseCommand(IConsole console)
    {
        _console = console;
    }

    [Option("-q|--quiet", Description = "Suppress normal output messages. This does not suppress errors")]
    [UsedImplicitly]
    public bool IsQuiet { get; set; }

    [UsedImplicitly]
    public virtual int OnExecute(CommandLineApplication app)
    {
        app.ShowHelp();
        return 1;
    }

    protected void Log(string message)
    {
        if (IsQuiet)
        {
            return;
        }

        _console.WriteLine(message);
    }

    protected void LogError(string message)
    {
        _console.Error.WriteLine(message);
    }
}
