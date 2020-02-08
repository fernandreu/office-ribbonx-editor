using System;
using GalaSoft.MvvmLight;
using OfficeRibbonXEditor.Helpers;

namespace OfficeRibbonXEditor.ViewModels.Shell
{
    public class FileAssociationViewModel : ViewModelBase
    {
        public FileAssociationViewModel(string extension)
        {
            this.Extension = extension;
            this.PreviousValue = GetCurrentValue();
            this.NewValue = this.PreviousValue;
        }

        public event EventHandler? ValueChanged; 

        public string Extension { get; }

        private bool PreviousValue { get; set; }

        private bool newValue;

        public bool NewValue
        {
            get => this.newValue;
            set
            {
                if (!this.Set(ref this.newValue, value))
                {
                    return;
                }

                this.ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool GetCurrentValue()
        {
            return new FileAssociationHelper(this.Extension).CheckAssociation();
        }

        public void Apply()
        {
            if (this.PreviousValue == this.NewValue)
            {
                return;
            }

            if (this.NewValue)
            {
                new FileAssociationHelper(this.Extension).AddAssociation();
            }
            else
            {
                new FileAssociationHelper(this.Extension).RemoveAssociation();
            }

            this.PreviousValue = this.NewValue;
        }

        public void ResetToCurrent()
        {
            this.NewValue = this.PreviousValue;
        }

        public void ResetToDefault()
        {
            this.NewValue = false;
        }
    }
}
