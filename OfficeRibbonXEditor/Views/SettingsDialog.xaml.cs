// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SettingsDialog.xaml.cs" company="FA">
//   Fernando Andreu
// </copyright>
// <summary>
//   Interaction logic for SettingsDialog.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace OfficeRibbonXEditor.Views
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;

    using OfficeRibbonXEditor.Models;

    /// <summary>
    /// Interaction logic for SettingsDialog
    /// </summary>
    public partial class SettingsDialog : Window
    {
        private readonly ScintillaLexer lexer;

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
            };

        private readonly Dictionary<string, object> currentValues = new Dictionary<string, object>();

        public SettingsDialog(ScintillaLexer lexer)
        {
            this.InitializeComponent();

            this.lexer = lexer;

            this.WrapModeBox.ItemsSource = Enum.GetValues(typeof(ScintillaNET.WrapMode)).Cast<ScintillaNET.WrapMode>();

            this.LoadCurrent();
        }
        
        public bool IsAccepted { get; set; }

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
            this.lexer.Update();
        }

        private void ApplySettings(object sender, RoutedEventArgs e)
        {
            this.ApplySettings();
        }

        private void AcceptSettings(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Save();
            this.IsAccepted = true;
            this.lexer.Update();
            this.Close();
        }

        private void ResetSettings(object sender, RoutedEventArgs e)
        {
            this.ResetToDefault();
        }

        private void WindowClosing(object sender, CancelEventArgs e)
        {
            if (!this.IsAccepted)
            {
                this.ResetToCurrent();
            }
        }
    }
}
