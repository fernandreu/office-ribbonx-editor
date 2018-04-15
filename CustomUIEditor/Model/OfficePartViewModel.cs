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

        private OfficePart part;
        
        public OfficePartViewModel(OfficePart part, OfficeDocumentViewModel parent) 
            : base(parent, false, contents: part.ReadContent())
        {
            this.part = part;
            this.originalContents = this.Contents;
        }

        public string OriginalContents => this.originalContents;

        public OfficePart Part => this.part;

        public bool HasUnsavedChanges => this.originalContents != this.Contents;

        public string Name => this.Part.Name;

        public string ImageSource => "/Resources/xml.png";  // TODO: That's probably not the only one possible

        /// <summary>
        /// Reloads the OfficePart associated with this view model, but preserving the current edits if not saved
        /// </summary>
        /// <returns>Whether it was reloaded successfully (it might not if the corresponding part type does not 
        /// exist in the document anymore)</returns>
        public bool Reload()
        {
            var reloaded = ((OfficeDocumentViewModel)this.Parent).Document.RetrieveCustomPart(this.Part.PartType);
            if (reloaded == null)
            {
                // Don't do anything if there is no equivalent to reload
                return false;
            }

            this.part = reloaded;
            this.originalContents = reloaded.ReadContent();

            return true;
        }

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
