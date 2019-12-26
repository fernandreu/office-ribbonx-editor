using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using GalaSoft.MvvmLight.Command;
using OfficeRibbonXEditor.Interfaces;
using OfficeRibbonXEditor.Properties;
using OfficeRibbonXEditor.ViewModels.Shell;
using OfficeRibbonXEditor.ViewModels.Tabs;

namespace OfficeRibbonXEditor.ViewModels.Dialogs
{

    public class SettingsDialogViewModel : DialogBase, IContentDialog<ICollection<ITabItemViewModel>>
    {
        private static readonly ICollection<string> extensions = new List<string>
        {
            ".xlsx",
            ".xlsm",
        };

        private readonly string[] usedProperties =
        {
            nameof(Settings.Default.TextColor),
            nameof(Settings.Default.BackgroundColor),
            nameof(Settings.Default.TagColor),
            nameof(Settings.Default.AttributeColor),
            nameof(Settings.Default.CommentColor),
            nameof(Settings.Default.StringColor),
            nameof(Settings.Default.EditorFontSize),
            nameof(Settings.Default.TabWidth),
            nameof(Settings.Default.WrapMode),
            nameof(Settings.Default.AutoIndent),
            nameof(Settings.Default.PreserveAttributes),
            nameof(Settings.Default.ShowDefaultSamples),
            nameof(Settings.Default.CustomSamples),
        };
        
        private readonly Dictionary<string, object> currentValues = new Dictionary<string, object>();

        public SettingsDialogViewModel()
        {
            this.ResetCommand = new RelayCommand(this.ResetToDefault);
            this.ApplyCommand = new RelayCommand(this.ApplySettings);
            this.AcceptCommand = new RelayCommand(this.AcceptSettings);
            this.SetAllAssociationsCommand = new RelayCommand<bool>(this.SetAllAssociations);

            foreach (var extension in extensions)
            {
                var association = new FileAssociationViewModel(extension);
                association.ValueChanged += (o, e) => this.SettingsChanged = true;
                this.FileAssociations.Add(association);
            }

            Settings.Default.PropertyChanged += (o, e) => this.SettingsChanged = true;
        }

        public bool OnLoaded(ICollection<ITabItemViewModel> payload)
        {
            this.Tabs = payload;
            this.LoadCurrent();
            return true;
        }

        private bool settingsChanged;

        public bool SettingsChanged
        {
            get => this.settingsChanged;
            set => this.Set(ref this.settingsChanged, value);
        }

        public ICollection<ITabItemViewModel> Tabs { get; private set; }

        public ICollection<FileAssociationViewModel> FileAssociations { get; } = new List<FileAssociationViewModel>();

        public RelayCommand ResetCommand { get; }

        public RelayCommand ApplyCommand { get; }

        public RelayCommand AcceptCommand { get; }

        public RelayCommand<bool> SetAllAssociationsCommand { get; }

        private void LoadCurrent()
        {
            this.currentValues.Clear();

            foreach (var name in this.usedProperties)
            {
                this.currentValues[name] = Settings.Default[name];
            }
        }

        private void ResetToCurrent()
        {
            foreach (var pair in this.currentValues)
            {
                Settings.Default[pair.Key] = pair.Value;
            }

            foreach (var association in this.FileAssociations)
            {
                association.ResetToCurrent();
            }
        }

        private void ResetToDefault()
        {
            foreach (var name in this.usedProperties)
            {
                var propertyValue = Settings.Default.PropertyValues[name];

                propertyValue.PropertyValue = propertyValue.Property.DefaultValue;
                propertyValue.Deserialized = false;

                Settings.Default[name] = propertyValue.PropertyValue;
            }

            foreach (var association in this.FileAssociations)
            {
                association.ResetToDefault();
            }

            this.ApplySettings();
        }

        private void ApplySettings()
        {
            Settings.Default.Save();
            this.LoadCurrent();
            foreach (var tab in this.Tabs.OfType<EditorTabViewModel>())
            {
                tab.Lexer?.Update();
            }

            foreach (var association in this.FileAssociations)
            {
                association.Apply();
            }

            this.SettingsChanged = false;
        }

        private void AcceptSettings()
        {
            this.ApplySettings();
            this.IsCancelled = false;
            this.Close();
        }

        private void SetAllAssociations(bool newValue)
        {
            foreach (var association in this.FileAssociations)
            {
                association.NewValue = newValue;
            }
        }

        protected override void OnClosing(CancelEventArgs args)
        {
            if (!args.Cancel && this.IsCancelled)
            {
                this.ResetToCurrent();
            }
        }
    }
}
