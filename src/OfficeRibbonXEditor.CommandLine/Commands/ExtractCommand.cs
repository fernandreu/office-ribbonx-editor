using McMaster.Extensions.CommandLineUtils;

namespace OfficeRibbonXEditor.CommandLine.Commands;

[Command(Description = "Extracts the custom UI files from an Office file")]
public class ExtractCommand : BaseUpdateCommand
{
    public ExtractCommand(IConsole console) : base(console)
    {
    }
}
