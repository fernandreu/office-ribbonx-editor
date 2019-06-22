using System;
using System.Windows;
using OfficeRibbonXEditor.Controls;
using OfficeRibbonXEditor.Interfaces;
using OfficeRibbonXEditor.ViewModels;
using Xceed.Wpf.Toolkit.Core.Converters;

namespace OfficeRibbonXEditor.Views
{
    public class DialogHostBase : Window
    {
        public static readonly DependencyProperty ViewProperty = DependencyProperty.Register(
            nameof(View),
            typeof(DialogControl),
            typeof(DialogHostBase));

        public DialogControl View
        {
            get => (DialogControl) this.GetValue(ViewProperty);
            set => this.SetValue(ViewProperty, value);
        }

        public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(
            nameof(Model),
            typeof(IContentDialogBase),
            typeof(DialogHostBase),
            new FrameworkPropertyMetadata(OnModelChanged));

        public IContentDialogBase Model
        {
            get => (IContentDialogBase) this.GetValue(ModelProperty);
            set => this.SetValue(ModelProperty, value);
        }

        private static void OnModelChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(sender is DialogHost host))
            {
                return;
            }

            if (e.NewValue == null)
            {
                host.View = null;
                return;
            }

            var view = GenerateControl(e.NewValue.GetType());
            view.DataContext = e.NewValue;
            host.View = view;
            host.Content = view;
            host.SizeToContent = SizeToContent.Manual;
            // TODO: This does not work as expected in WPF (as opposed to Windows Forms), so disabled for now
            // The main reason is that the window can only be made transparent if it has WindowStyle set to None,
            // which means the dialog will need its own border, close controls, etc, and would lose the look and
            // feel of the main window.
            ////host.Deactivated += (o, args) => host.Opacity = view.InactiveOpacity;
            ////host.Activated += (o, args) => host.Opacity = 1.0;

            // What follows is done here and not in XAML bindings because there can be sizing issues otherwise

            if (view.PreferredWidth > 0)
            {
                host.Width = view.PreferredWidth;
            }

            if (view.PreferredHeight > 0)
            {
                host.Height = view.PreferredHeight;
            }

            host.SizeToContent = view.SizeToContent;
            host.CenterInOwner();
        }

        private static DialogControl GenerateControl(Type contentDialogType)
        {
            if (contentDialogType == typeof(AboutDialogViewModel))
            {
                return new AboutDialog();
            }

            if (contentDialogType == typeof(CallbackDialogViewModel))
            {
                return new CallbackDialog();
            }

            if (contentDialogType == typeof(SettingsDialogViewModel))
            {
                return new SettingsDialog();
            }

            if (contentDialogType == typeof(GoToDialogViewModel))
            {
                return new GoToDialog();
            }

            throw new ArgumentException($"Type {contentDialogType.Name} does not have an registered control");
        }

        private void CenterInOwner()
        {
            if (this.Owner == null)
            {
                return;
            }

            this.Left = this.Owner.Left - (this.Width - this.Owner.Width) / 2;
            this.Top = this.Owner.Top - (this.Height - this.Owner.Height) / 2;
        }
    }
}
