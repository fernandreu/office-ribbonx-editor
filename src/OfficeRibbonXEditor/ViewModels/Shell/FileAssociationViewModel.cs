using System;
using GalaSoft.MvvmLight;
using OfficeRibbonXEditor.Helpers;

namespace OfficeRibbonXEditor.ViewModels.Shell
{
    public class FileAssociationViewModel : ViewModelBase
    {
        public FileAssociationViewModel(string extension)
        {
            Extension = extension;
            PreviousValue = GetCurrentValue();
            NewValue = PreviousValue;
        }

        public event EventHandler? ValueChanged; 

        public string Extension { get; }

        private bool PreviousValue { get; set; }

        private bool _newValue;
        public bool NewValue
        {
            get => _newValue;
            set
            {
                if (!Set(ref _newValue, value))
                {
                    return;
                }

                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool GetCurrentValue()
        {
            return new FileAssociationHelper(Extension).CheckAssociation();
        }

        public void Apply()
        {
            if (PreviousValue == NewValue)
            {
                return;
            }

            if (NewValue)
            {
                new FileAssociationHelper(Extension).AddAssociation();
            }
            else
            {
                new FileAssociationHelper(Extension).RemoveAssociation();
            }

            PreviousValue = NewValue;
        }

        public void ResetToCurrent()
        {
            NewValue = PreviousValue;
        }

        public void ResetToDefault()
        {
            NewValue = false;
        }
    }
}
