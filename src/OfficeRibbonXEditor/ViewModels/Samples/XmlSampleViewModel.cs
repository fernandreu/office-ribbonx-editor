using GalaSoft.MvvmLight;
using OfficeRibbonXEditor.Interfaces;

namespace OfficeRibbonXEditor.ViewModels.Samples
{
    public abstract class XmlSampleViewModel : ViewModelBase, ISampleMenuItem
    {
        public abstract string Name { get; }

        public abstract string ReadContents();
    }
}
