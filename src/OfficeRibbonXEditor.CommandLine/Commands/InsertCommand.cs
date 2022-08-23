using CommunityToolkit.Diagnostics;
using McMaster.Extensions.CommandLineUtils;
using OfficeRibbonXEditor.Documents;
using System.ComponentModel.DataAnnotations;

namespace OfficeRibbonXEditor.CommandLine.Commands;

[Command(Description = "Inserts a custom UI file to an Office file. If it already exists, it will be overwritten")]
public class InsertCommand : BaseUpdateCommand
{
    public InsertCommand(IConsole console) : base(console)
    {
    }

    [Option("--type <TYPE>", Description = "The type of Custom UI file. If not given, it will be inferred from the filename")]
    [AllowedValues("12", "14")]
    public string? Type { get; set; }

    [Option("-x|--xml <FILE>", Description = "The custom UI xml file to be inserted")]
    [FileExists]
    [Required]
    public string? CustomUiFile { get; set; }

    [Option("-i|--icons <FOLDER>", Description = "A folder with images to be inserted together with the custom UI file. The file names will be used as IDs")]
    [DirectoryExists]
    public string? IconsFolder { get; set; }

    public override int OnExecute(CommandLineApplication app)
    {
        Guard.IsNotNull(CustomUiFile);
        Guard.IsNotNull(OfficeFile);

        Type ??= CustomUiFile.EndsWith("14.xml", StringComparison.OrdinalIgnoreCase) ? "14" : "12";
        var partType = Type == "12" ? XmlPart.RibbonX12 : XmlPart.RibbonX14;

        Log($"Opening Office file '{Path.GetFileName(OfficeFile)}'...");
        var doc = new OfficeDocument(OfficeFile);

        Log($"Inserting Custom UI {Type} part from file '{Path.GetFileName(CustomUiFile)}'...");
        var part = doc.RetrieveCustomPart(partType) ?? doc.CreateCustomPart(partType);
        var content = File.ReadAllText(CustomUiFile);
        part.Save(content);

        if (IconsFolder != null)
        {
            Log($"Inserting icons from folder '{Path.GetFileName(IconsFolder)}'...");
            foreach (var fi in new DirectoryInfo(IconsFolder).GetFiles())
            {
                part.AddImage(fi.FullName);
            }
        }

        doc.Save(OutputFile);
        return 0;
    }
}
