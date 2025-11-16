using McMaster.Extensions.CommandLineUtils;

namespace OfficeRibbonXEditor.CommandLine.Commands;

[Command(Description = "Extracts the custom UI files from an Office file")]
public class ExtractCommand(IConsole console) : BaseUpdateCommand(console);
