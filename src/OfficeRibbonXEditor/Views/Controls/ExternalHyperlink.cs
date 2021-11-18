using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Navigation;
using GalaSoft.MvvmLight.Command;
using OfficeRibbonXEditor.Services;

namespace OfficeRibbonXEditor.Views.Controls
{
    /// <summary>
    /// Hyperlink which automatically takes care of opening its Uri in an external browser
    /// </summary>
    public class ExternalHyperlink : Hyperlink
    {
        public static readonly DependencyProperty OpenUrlCommandProperty = DependencyProperty.Register(
            nameof(OpenUrlCommand),
            typeof(RelayCommand<Uri>),
            typeof(DialogControl));

        public RelayCommand<Uri> OpenUrlCommand
        {
            get => (RelayCommand<Uri>)GetValue(OpenUrlCommandProperty);
            set => SetValue(OpenUrlCommandProperty, value);
        }

        public ExternalHyperlink()
        {
            // We give this the default value we will want in most cases while
            // still allowing it to be defined via xaml if needed
            OpenUrlCommand = new RelayCommand<Uri>(uri => new UrlHelper().OpenExternal(uri));

            RequestNavigate += OnRequestNavigate;
        }

        private void OnRequestNavigate(object? sender, RequestNavigateEventArgs e)
        {
            OpenUrlCommand.Execute(e.Uri);
            e.Handled = true;
        }
    }
}
