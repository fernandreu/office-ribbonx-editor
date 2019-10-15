using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OfficeRibbonXEditor.Interfaces;
using OfficeRibbonXEditor.Models;
using OfficeRibbonXEditor.ViewModels;
using ScintillaNET;

namespace OfficeRibbonXEditor.Controls
{
    /// <summary>
    /// Interaction logic for EditorTab.xaml
    /// </summary>
    public partial class IconTab : UserControl
    {
        private IconTabViewModel viewModel;

        public IconTab()
        {
            this.InitializeComponent();
        }
        
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            base.OnPropertyChanged(args);

            if (args.Property != DataContextProperty)
            {
                return;
            }

            if (args.OldValue is IconTabViewModel previousModel)
            {
                // TODO: Remove listeners
            }

            if (!(args.NewValue is IconTabViewModel model))
            {
                this.viewModel = null;
                return;
            }

            this.viewModel = model;

            // TODO: Add listeners
        }
    }
}
