// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OfficePartViewModel.cs" company="FA">
//   Fernando Andreu
// </copyright>
// <summary>
//   Defines the OfficePartViewModel type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CustomUIEditor.Model
{
    using CustomUIEditor.Data;

    public class OfficePartViewModel : TreeViewItemViewModel
    {
        private string originalContents;
        
        public OfficePartViewModel(OfficePart part, OfficeDocumentViewModel parent) 
            : base(parent, false, contents: part.ReadContent())
        {
            this.Part = part;
            this.originalContents = this.Contents;
        }

        public string OriginalContents => this.originalContents;
        
        public OfficePart Part { get; set; }

        public bool HasUnsavedChanges => this.originalContents != this.Contents;

        public string Name => this.Part.Name;

        public string ImageSource => "/Resources/xml.png";  // TODO: That's probably not the only one possible
        
        public void Save()
        {
            // Do not simply call Part.Save because that will not flag the part as dirty in the parent document
            var docModel = (OfficeDocumentViewModel)this.Parent;
            docModel.Document.SaveCustomPart(this.Part.PartType, this.Contents);
            this.originalContents = this.Contents;
        }

        protected override void SelectionLost()
        {
            // I rather prefer not saving them automatically, but only on request. That way it is also easier to check whether there are unsaved changes at any point
            ////if (HasUnsavedChanges)
            ////    SaveChanges();
        }
    }
}
