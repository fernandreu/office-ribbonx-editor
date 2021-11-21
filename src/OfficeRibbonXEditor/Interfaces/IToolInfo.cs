namespace OfficeRibbonXEditor.Interfaces
{
    public interface IToolInfo
    {
        string AssemblyTitle { get; }

        string AssemblyVersion { get; }

        string AssemblyDescription { get; }

        string AssemblyProduct { get; }

        string AssemblyCopyright { get; }

        string AssemblyCompany { get; }

        string RuntimeVersion { get; }

        string OperatingSystemVersion { get; }
    }
}
