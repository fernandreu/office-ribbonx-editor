// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XmlSampleViewModel.cs" company="FA">
//   Fernando Andreu
// </copyright>
// <summary>
//   Defines the XmlSampleViewModel type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CustomUIEditor.ViewModels
{
    using System.IO;

    using GalaSoft.MvvmLight;
    
    public class XmlSampleViewModel : ViewModelBase
    {
        public string FilePath { get; set; }

        public string Name => Path.GetFileNameWithoutExtension(this.FilePath);
    }
}
