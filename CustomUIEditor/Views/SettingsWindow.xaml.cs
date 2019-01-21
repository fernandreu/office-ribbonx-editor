// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SettingsWindow.xaml.cs" company="FA">
//   Fernando Andreu
// </copyright>
// <summary>
//   Interaction logic for SettingsWindow.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CustomUIEditor.Views
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    
    /// <summary>
    /// Interaction logic for SettingsWindow
    /// </summary>
    public partial class SettingsWindow : Window
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
                nameof(Properties.Settings.Default.WrapMode)
            };

        private readonly Dictionary<string, object> currentValues = new Dictionary<string, object>();

        public SettingsWindow()
        {
            this.InitializeComponent();

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
            if (this.Owner is MainWindow window)
            {
                window.SetScintillaLexer();
            }
        }

        private void ApplySettings(object sender, RoutedEventArgs e)
        {
            this.ApplySettings();
        }

        private void AcceptSettings(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Save();
            this.IsAccepted = true;
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
