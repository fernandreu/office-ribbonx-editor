using CommunityToolkit.Mvvm.ComponentModel;
using OfficeRibbonXEditor.Interfaces;

namespace OfficeRibbonXEditor.ViewModels.Samples;

public abstract class XmlSampleViewModel : ObservableObject, ISampleMenuItem
{
    public abstract string Name { get; }

    public abstract string ReadContents();
}