using McMaster.Extensions.CommandLineUtils;
using JetBrains.Annotations;

namespace OfficeRibbonXEditor.CommandLine.Commands;

public abstract class BaseUpdateCommand(IConsole console) : BaseCommand(console)
{
    [Option("-o|--output <FILE>", Description = "The resulting Office file after the updates. If not given, the file will be edited in-place")]
    [UsedImplicitly]
    public string? OutputFile { get; set; }

    [Argument(0, Description = "The target Office file")]
    [FileExists]
    [Required]
    [UsedImplicitly]
    public string? OfficeFile { get; set; }
}
