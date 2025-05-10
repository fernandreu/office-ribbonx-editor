using McMaster.Extensions.CommandLineUtils;
using OfficeRibbonXEditor.CommandLine.Commands;
using System.Diagnostics;
using JetBrains.Annotations;

namespace OfficeRibbonXEditor.CommandLine;

[Command(Name = "OfficeRibbonXEditor", Description = "A command-line interface to edit the custom UI of Office files")]
[Subcommand(typeof(ExtractCommand), typeof(InsertCommand))]
[UsedImplicitly]
public class Program : BaseCommand
{
    public Program(IConsole console) : base(console)
    {
    }

    public static int Main(string[] args)
        => CommandLineApplication.Execute<Program>(args);

    public override int OnExecute(CommandLineApplication app)
    {
        // This will only execute when no arguments are passed. As soon as one is
        // passed, either the corresponding command will handle them or the app
        // itself will handle the error due to not being recognized. So, we can
        // safely launch the GUI

        var exePath = Path.Combine(AppContext.BaseDirectory, "OfficeRibbonXEditor.exe");
        if (!File.Exists(exePath))
        {
            return base.OnExecute(app);
        }

        var p = Process.Start(exePath);
        p.WaitForExit();
        return p.ExitCode;
    }
}