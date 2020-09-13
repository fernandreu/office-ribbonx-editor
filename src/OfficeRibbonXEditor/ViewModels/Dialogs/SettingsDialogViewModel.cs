using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using GalaSoft.MvvmLight.Command;
using OfficeRibbonXEditor.Helpers;
using OfficeRibbonXEditor.Interfaces;
using OfficeRibbonXEditor.Properties;
using OfficeRibbonXEditor.ViewModels.Shell;
using OfficeRibbonXEditor.ViewModels.Tabs;
using WPFLocalizeExtension.Engine;

namespace OfficeRibbonXEditor.ViewModels.Dialogs
{

    public class SettingsDialogViewModel : DialogBase, IContentDialog<ICollection<ITabItemViewModel>>
    {
        private static readonly ICollection<string> extensions = new List<string>
        {
            ".docx",
            ".docm",
            ".dotx",
            ".dotm",
            ".pptx",
            ".pptm",
            ".ppsx",
            ".ppsm",
            ".potx",
            ".potm",
            ".ppam",
            ".xlsx",
            ".xlsm",
            ".xltm",
            ".xlam",
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
            nameof(Settings.Default.UICulture),
        };
        
        private readonly Dictionary<string, object> currentValues = new Dictionary<string, object>();

        public SettingsDialogViewModel()
        {
            this.LanguageNames = this.Languages.Select(x => x.Name).ToList();

            this.ResetToDefaultCommand = new RelayCommand(this.ResetToDefault);
            this.ResetToCurrentCommand = new RelayCommand(this.ResetToCurrent);
            this.ApplyCommand = new RelayCommand(this.ApplySettings);
            this.AcceptCommand = new RelayCommand(this.AcceptSettings);
            this.SetAllAssociationsCommand = new RelayCommand<bool>(this.SetAllAssociations);

            foreach (var extension in extensions)
            {
                var association = new FileAssociationViewModel(extension);
                association.ValueChanged += (o, e) => this.SettingsChanged = true;
                this.FileAssociations.Add(association);
            }

            this.LoadLanguage();

            Settings.Default.PropertyChanged += this.SettingsChangedEventHandler;
        }

        private string? language;

        public string? Language
        {
            get => this.language;
            set
            {
                if (!this.Set(ref this.language, value))
                {
                    return;
                }

                this.SaveLanguage();
            }
        }

        private bool settingsChanged;

        public bool SettingsChanged
        {
            get => this.settingsChanged;
            set => this.Set(ref this.settingsChanged, value);
        }

        private bool languageChanged;

        public bool LanguageChanged
        {
            get => this.languageChanged;
            set => this.Set(ref this.languageChanged, value);
        }

        public ICollection<ITabItemViewModel> Tabs { get; } = new List<ITabItemViewModel>();

        public ICollection<FileAssociationViewModel> FileAssociations { get; } = new List<FileAssociationViewModel>();

        // Note: Not using this directly for the ComboBox items binding (plus DisplayMemberPath="Name") because it will
        // cause issues with the UI testing. Using the raw strings in LanguageNames instead
        public ICollection<LanguageChoice> Languages { get; } = new[]
        {
            new LanguageChoice("English", string.Empty), 
            new LanguageChoice("Español", "es"),
            new LanguageChoice("Français", "fr"),
        };

        public ICollection<string> LanguageNames { get; }

        public RelayCommand ResetToDefaultCommand { get; }

        public RelayCommand ResetToCurrentCommand { get; }

        public RelayCommand ApplyCommand { get; }

        public RelayCommand AcceptCommand { get; }

        public RelayCommand<bool> SetAllAssociationsCommand { get; }

        public bool OnLoaded(ICollection<ITabItemViewModel> payload)
        {
            this.Tabs.Clear();
            foreach (var tab in payload)
            {
                this.Tabs.Add(tab);
            }

            this.LoadCurrent();
            return true;
        }

        private void SettingsChangedEventHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Settings.Default.UICulture))
            {
                this.LoadLanguage();
                this.LanguageChanged = true;
            }

            this.SettingsChanged = true;
        }

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

            if (this.LanguageChanged)
            {
                LocalizeDictionary.Instance.Culture
                    = Thread.CurrentThread.CurrentUICulture
                        = new CultureInfo(Settings.Default.UICulture);
            }

            this.SettingsChanged = false;
            this.LanguageChanged = false;
        }

        private void AcceptSettings()
        {
            this.ApplySettings();
            this.IsCancelled = false;
            this.Close();
        }

        private void LoadLanguage()
        {
            var foundLanguage = this.Languages.FirstOrDefault(x => x.Id == Settings.Default.UICulture);
            this.Language = foundLanguage?.Name ?? this.Languages.First().Name;
        }

        private void SaveLanguage()
        {
            var foundLanguage = this.Languages.FirstOrDefault(x => x.Name == this.Language);
            Settings.Default.UICulture = foundLanguage.Id;
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
