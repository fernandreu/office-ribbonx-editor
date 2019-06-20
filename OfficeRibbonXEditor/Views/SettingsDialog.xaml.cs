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
    using System.Windows.Controls;
    using OfficeRibbonXEditor.Models;

    /// <summary>
    /// Interaction logic for SettingsDialog
    /// </summary>
    public partial class SettingsDialog : UserControl  // TODO: Move all this code to viewmodel
    {
        public SettingsDialog()
        {
            this.InitializeComponent();

            this.WrapModeBox.ItemsSource = Enum.GetValues(typeof(ScintillaNET.WrapMode)).Cast<ScintillaNET.WrapMode>();
        }
    }
}
