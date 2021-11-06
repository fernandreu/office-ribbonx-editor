using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using GalaSoft.MvvmLight.Command;
using Generators;
using OfficeRibbonXEditor.Helpers;
using OfficeRibbonXEditor.Interfaces;
using OfficeRibbonXEditor.Properties;
using OfficeRibbonXEditor.ViewModels.Shell;
using OfficeRibbonXEditor.ViewModels.Tabs;
using WPFLocalizeExtension.Engine;

namespace OfficeRibbonXEditor.ViewModels.Dialogs
{
    [Export]
    public partial class SettingsDialogViewModel : DialogBase, IContentDialog<ICollection<ITabItemViewModel>>
    {
        public static readonly ICollection<CultureInfo> LanguageChoices = new[]
        {
            new CultureInfo("en-US"),
            new CultureInfo("de-DE"),
            new CultureInfo("es-ES"),
            new CultureInfo("fr-FR"),
            new CultureInfo("nl-NL"),
            new CultureInfo("tr-TR"),
            new CultureInfo("zh-CN"),
        };

        private static readonly ICollection<string> Extensions = new List<string>
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
            ".vsdx",
            ".vsdm",
            ".vstx",
            ".vstm",
        };

        private readonly string[] _usedProperties =
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

        private readonly Dictionary<string, object> _currentValues = new Dictionary<string, object>();

        public SettingsDialogViewModel()
        {
            LanguageNames = LanguageChoices.Select(x => x.TextInfo.ToTitleCase(x.NativeName)).ToList();

            foreach (var extension in Extensions)
            {
                var association = new FileAssociationViewModel(extension);
                association.ValueChanged += (o, e) => SettingsChanged = true;
                FileAssociations.Add(association);
            }

            _language = LoadLanguage();

            Settings.Default.PropertyChanged += SettingsChangedEventHandler;
        }

        private string? _language;
        public string? Language
        {
            get => _language;
            set
            {
                if (!Set(ref _language, value))
                {
                    return;
                }

                SaveLanguage();
            }
        }

        private bool _settingsChanged;
        public bool SettingsChanged
        {
            get => _settingsChanged;
            set => Set(ref _settingsChanged, value);
        }

        private bool _languageChanged;
        public bool LanguageChanged
        {
            get => _languageChanged;
            set => Set(ref _languageChanged, value);
        }

        public ICollection<ITabItemViewModel> Tabs { get; } = new List<ITabItemViewModel>();

        public ICollection<FileAssociationViewModel> FileAssociations { get; } = new List<FileAssociationViewModel>();

        // Note: Not using directly LanguageChoice as ComboBox items binding (plus DisplayMemberPath="Name") because it will
        // cause issues with the UI testing. Using a raw string list instead
        public ICollection<string> LanguageNames { get; }

        public bool OnLoaded(ICollection<ITabItemViewModel> payload)
        {
            Tabs.Clear();
            foreach (var tab in payload)
            {
                Tabs.Add(tab);
            }

            LoadCurrent();
            return true;
        }

        private void SettingsChangedEventHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Settings.Default.UICulture))
            {
                Language = LoadLanguage();
                LanguageChanged = true;
            }

            SettingsChanged = true;
        }

        private void LoadCurrent()
        {
            _currentValues.Clear();

            foreach (var name in _usedProperties)
            {
                _currentValues[name] = Settings.Default[name];
            }
        }

        [GenerateCommand]
        private void ResetToCurrent()
        {
            foreach (var pair in _currentValues)
            {
                Settings.Default[pair.Key] = pair.Value;
            }

            foreach (var association in FileAssociations)
            {
                association.ResetToCurrent();
            }
        }

        [GenerateCommand]
        private void ResetToDefault()
        {
            foreach (var name in _usedProperties)
            {
                var propertyValue = Settings.Default.PropertyValues[name];

                propertyValue.PropertyValue = propertyValue.Property.DefaultValue;
                propertyValue.Deserialized = false;

                Settings.Default[name] = propertyValue.PropertyValue;
            }

            foreach (var association in FileAssociations)
            {
                association.ResetToDefault();
            }

            ApplySettings();
        }

        [GenerateCommand(Name = "ApplyCommand")]
        private void ApplySettings()
        {
            Settings.Default.Save();
            LoadCurrent();
            foreach (var tab in Tabs.OfType<EditorTabViewModel>())
            {
                tab.Lexer?.Update();
            }

            foreach (var association in FileAssociations)
            {
                association.Apply();
            }

            if (LanguageChanged)
            {
                LocalizeDictionary.Instance.Culture
                    = CultureInfo.CurrentUICulture
                        = CultureInfo.DefaultThreadCurrentUICulture
                            = new CultureInfo(Settings.Default.UICulture);
            }

            SettingsChanged = false;
            LanguageChanged = false;
        }

        [GenerateCommand(Name = "AcceptCommand")]
        private void AcceptSettings()
        {
            ApplySettings();
            IsCancelled = false;
            Close();
        }

        private string LoadLanguage()
        {
            try
            {
                var foundLanguage = LanguageChoices.FirstOrDefault(x => x.ThreeLetterISOLanguageName == new CultureInfo(Settings.Default.UICulture).ThreeLetterISOLanguageName);
                var culture = foundLanguage ?? LanguageChoices.First();
                return culture.TextInfo.ToTitleCase(culture.NativeName);
            }
            catch (CultureNotFoundException)
            {
                // The setting was probably incorrect. Perhaps a corrupted settings file, or a development version of the tool before the system was changed slightly
                return LanguageNames.First();
            }
        }

        private void SaveLanguage()
        {
            // We do not check the ThreeLetterISOLanguageName in this case because all options should come from the dropdown
            var foundLanguage = LanguageChoices.FirstOrDefault(x => x.TextInfo.ToTitleCase(x.NativeName) == Language);
            Settings.Default.UICulture = foundLanguage.Name;
        }

        [GenerateCommand]
        private void SetAllAssociations(bool newValue)
        {
            foreach (var association in FileAssociations)
            {
                association.NewValue = newValue;
            }
        }

        protected override void OnClosing(CancelEventArgs args)
        {
            if (!args.Cancel && IsCancelled)
            {
                ResetToCurrent();
            }
        }
    }
}
