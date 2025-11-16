using CommunityToolkit.Mvvm.ComponentModel;
using OfficeRibbonXEditor.Helpers;

namespace OfficeRibbonXEditor.ViewModels.Shell;

public class FileAssociationViewModel : ObservableObject
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

    public bool NewValue
    {
        get;
        set
        {
            if (!SetProperty(ref field, value))
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