using System.Collections.Generic;
using System.ComponentModel;
using GalaSoft.MvvmLight.Command;
using OfficeRibbonXEditor.Interfaces;
using OfficeRibbonXEditor.Models;

namespace OfficeRibbonXEditor.ViewModels
{

    public class SettingsDialogViewModel : DialogBase, IContentDialog<ICollection<EditorTabViewModel>>
    {
        private readonly string[] usedProperties =
        {
            nameof(Properties.Settings.Default.TextColor),
            nameof(Properties.Settings.Default.BackgroundColor),
            nameof(Properties.Settings.Default.TagColor),
            nameof(Properties.Settings.Default.AttributeColor),
            nameof(Properties.Settings.Default.CommentColor),
            nameof(Properties.Settings.Default.StringColor),
            nameof(Properties.Settings.Default.EditorFontSize),
            nameof(Properties.Settings.Default.TabWidth),
            nameof(Properties.Settings.Default.WrapMode),
            nameof(Properties.Settings.Default.AutoIndent),
            nameof(Properties.Settings.Default.PreserveAttributes),
        };
        
        private readonly Dictionary<string, object> currentValues = new Dictionary<string, object>();

        public SettingsDialogViewModel()
        {
            this.ResetCommand = new RelayCommand(this.ResetToDefault);
            this.ApplyCommand = new RelayCommand(this.ApplySettings);
            this.AcceptCommand = new RelayCommand(this.AcceptSettings);
        }

        public bool OnLoaded(ICollection<EditorTabViewModel> payload)
        {
            this.Tabs = payload;
            this.LoadCurrent();
            return true;
        }

        public ICollection<EditorTabViewModel> Tabs { get; private set; }

        public RelayCommand ResetCommand { get; }

        public RelayCommand ApplyCommand { get; }

        public RelayCommand AcceptCommand { get; }

        private void LoadCurrent()
        {
            this.currentValues.Clear();

            foreach (var name in this.usedProperties)
            {
                this.currentValues[name] = Properties.Settings.Default[name];
            }
        }

        private void ResetToCurrent()
        {
            foreach (var pair in this.currentValues)
            {
                Properties.Settings.Default[pair.Key] = pair.Value;
            }
        }

        private void ResetToDefault()
        {
            foreach (var name in this.usedProperties)
            {
                var propertyValue = Properties.Settings.Default.PropertyValues[name];

                propertyValue.PropertyValue = propertyValue.Property.DefaultValue;
                propertyValue.Deserialized = false;

                Properties.Settings.Default[name] = propertyValue.PropertyValue;
            }

            this.ApplySettings();
        }

        private void ApplySettings()
        {
            Properties.Settings.Default.Save();
            this.LoadCurrent();
            foreach (var tab in this.Tabs)
            {
                tab.Lexer?.Update();
            }
        }

        private void AcceptSettings()
        {
            Properties.Settings.Default.Save();
            this.IsCancelled = false;
            foreach (var tab in this.Tabs)
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
