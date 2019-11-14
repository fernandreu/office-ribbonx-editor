using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using GalaSoft.MvvmLight.Command;
using OfficeRibbonXEditor.Interfaces;
using OfficeRibbonXEditor.Models;
using OfficeRibbonXEditor.Properties;

namespace OfficeRibbonXEditor.ViewModels
{

    public class SettingsDialogViewModel : DialogBase, IContentDialog<ICollection<ITabItemViewModel>>
    {
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

        public RelayCommand ResetCommand { get; }

        public RelayCommand ApplyCommand { get; }

        public RelayCommand AcceptCommand { get; }

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

            this.SettingsChanged = false;
        }

        private void AcceptSettings()
        {
            Settings.Default.Save();
            this.IsCancelled = false;
            foreach (var tab in this.Tabs.OfType<EditorTabViewModel>())
            {
                tab.Lexer?.Update();
            }
            this.Close();
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
